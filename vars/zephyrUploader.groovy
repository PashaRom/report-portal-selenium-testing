/**
 * Entry point from Jenkinsfile
 *
 * Usage:
 *   zephyrUploader(
 *       resultsPattern : 'path/to/test-results/** /junit.xml',
 *       projectKey     : 'KAN',
 *       jiraBaseUrl    : 'https://company.atlassian.net',
 *       jiraTokenId    : 'JIRA_TOKEN',
 *       zephyrTokenId  : 'ZEPHYR_TOKEN',
 *       createBugs     : true,   // optional, default: true
 *       dryRun         : false   // optional, default: false
 *   )
 */

def call(Map config = [:]) {

    // ── Validation of required parameters ──────────────────────────────────
    ['resultsPattern', 'projectKey', 'jiraBaseUrl', 'jiraTokenId', 'zephyrTokenId'].each { key ->
        if (!config[key]) {
            error("zephyrUploader: required parameter '${key}' is not set")
        }
    }

    def createBugs = config.get('createBugs', true)
    def dryRun     = config.get('dryRun', false)

    withCredentials([
        string(credentialsId: config.zephyrTokenId, variable: 'ZEPHYR_TOKEN'),
        string(credentialsId: config.jiraTokenId,   variable: 'JIRA_TOKEN')
    ]) {
        def zephyrToken = env.ZEPHYR_TOKEN
        def jiraToken   = env.JIRA_TOKEN

        // ── 1. Find all junit.xml files ─────────────────────────────────────────
        def xmlFiles = findFiles(glob: config.resultsPattern)
        if (!xmlFiles || xmlFiles.length == 0) {
            echo "⚠️  No files found for pattern: ${config.resultsPattern}"
            return
        }

        echo "📂 Found result files: ${xmlFiles.length}"

        xmlFiles.each { xmlFile ->
            echo "──────────────────────────────────────────────────"
            echo "📄 Processing: ${xmlFile.path}"

            // ── 2. Upload to Zephyr ──────────────────────────────────────
            if (!dryRun) {
                zephyrClient.uploadResults(zephyrToken, config.projectKey, xmlFile.path)
                echo "✅ Uploaded to Zephyr: ${xmlFile.path}"
            } else {
                echo "[DryRun] Upload to Zephyr: ${xmlFile.path}"
            }

            if (!createBugs) return

            // ── 3. Parse failed tests ────────────────────────────────────
            def xmlContent  = readFile(xmlFile.path)
            def failedTests = jUnitParser.parseFailedTests(xmlContent)

            if (!failedTests) {
                echo "✅ No failed tests in: ${xmlFile.path}"
                return
            }

            echo "❌ Failed tests: ${failedTests.size()}"

            failedTests.each { test ->
                processFailedTest(
                    test, zephyrToken, jiraToken,
                    config, dryRun
                )
            }
        }
    }
}

// ─── Processing a single failed test ──────────────────────────────────────────
private def processFailedTest(Map test, String zephyrToken, String jiraToken,
                               Map config, boolean dryRun) {
    def summary     = "[AUTO] Test Failed: ${test.className}.${test.testName}"
    def description = buildDescription(test)

    echo "  🔍 Checking for duplicate bug for: ${test.testName}"

    // ── 4. Check for duplicate ──────────────────────────────────────────────
    def existingBug = dryRun ? null
        : jiraClient.findOpenBugBySummary(config.jiraBaseUrl, jiraToken, config.projectKey, summary)

    String bugKey
    if (existingBug) {
        bugKey = existingBug.key
        echo "  ♻️  Open bug already exists: ${bugKey} — skipping creation"
    } else {
        // ── 5. Create bug ─────────────────────────────────────────────────
        if (!dryRun) {
            def created = jiraClient.createBug(
                config.jiraBaseUrl, jiraToken,
                config.projectKey, summary, description
            )
            bugKey = created.key
            echo "  🐛 Bug created: ${bugKey}"
        } else {
            echo "  [DryRun] Create bug: ${summary}"
            bugKey = 'DRY-RUN'
        }
    }

    // ── 6. Link bug to Zephyr test case ──────────────────────────────
    def testCase = dryRun ? null
        : zephyrClient.findTestCaseByName(zephyrToken, config.projectKey, test.testName)

    if (testCase) {
        def tcKey = testCase.key
        echo "  🔗 Linking bug ${bugKey} to test case: ${tcKey}"
        if (!dryRun) {
            zephyrClient.linkIssueToTestCase(zephyrToken, tcKey, bugKey)  // Zephyr link
            jiraClient.linkIssues(config.jiraBaseUrl, jiraToken, bugKey, tcKey) // Jira link
        }
    } else {
        echo "  ⚠️  Test case '${test.testName}' not found in Zephyr"
    }
}

// ─── Building bug description ───────────────────────────────────────────────
private def buildDescription(Map test) {
    return "Automatically created based on CI/CD results. " +
           "Test: ${test.className}.${test.testName}. " +
           "Error: ${test.type ?: 'N/A'}. " +
           "Message: ${test.message ?: 'N/A'}. " +
           "Stack Trace: ${test.stackTrace ?: 'N/A'}. " +
           "Build: ${env.BUILD_URL ?: 'N/A'}. " +
           "Job: ${env.JOB_NAME ?: 'N/A'} #${env.BUILD_NUMBER ?: 'N/A'}."
}