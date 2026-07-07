/**
 * Entry point from Jenkinsfile
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

        // ── 1. Find all junit.xml files using sh find ───────────────────────────
        // resultsPattern is the search directory, for example:
        // 'SeleniumFrameworkInteraction/UITests/test-results'
        def rawFiles = sh(
            script: "find ${config.resultsPattern} -name 'junit.xml' 2>/dev/null || true",
            returnStdout: true
        ).trim()

        if (!rawFiles) {
            echo "⚠️  No junit.xml files found in: ${config.resultsPattern}"
            return
        }

        def xmlFiles = rawFiles.split('\n').findAll { it.trim() }
        echo "📂 Found result files: ${xmlFiles.size()}"

        xmlFiles.each { filePath ->
            filePath = filePath.trim()
            echo "──────────────────────────────────────────────────"
            echo "📄 Processing: ${filePath}"

            // ── 2. Upload to Zephyr ──────────────────────────────────────
            if (!dryRun) {
                zephyrClient.uploadResults(config.projectKey, filePath)
                echo "✅ Uploaded to Zephyr: ${filePath}"
            } else {
                echo "[DryRun] Upload to Zephyr: ${filePath}"
            }

            if (!createBugs) return

            // ── 3. Parse failed tests ────────────────────────────────────
            def failedTests = jUnitParser.parseFailedTests(filePath)

            if (!failedTests || failedTests.isEmpty()) {
                echo "✅ No failed tests in: ${filePath}"
                return
            }

            echo "❌ Failed tests: ${failedTests.size()}"

            failedTests.each { test ->
                processFailedTest(test, config, dryRun)
            }
        }
    }
}

// ─── Processing a single failed test ──────────────────────────────────────────
private def processFailedTest(Map test, Map config, boolean dryRun) {
    def summary     = '[AUTO] Test Failed: ' + test.className + '.' + test.testName
    def description = buildDescription(test)

    echo "  🔍 Checking for duplicate bug for: ${test.testName}"

    // ── Проверка дубликата — возвращает String key или null ───────────────
    String existingBugKey = dryRun ? null
        : jiraClient.findOpenBugBySummary(config.jiraBaseUrl, config.projectKey, summary)

    String bugKey
    if (existingBugKey) {
        bugKey = existingBugKey
        echo "  ♻️  Open bug already exists: ${bugKey} — skipping creation"
    } else {
        if (!dryRun) {
            // createBug возвращает String key напрямую
            bugKey = jiraClient.createBug(config.jiraBaseUrl, config.projectKey, summary, description)
            echo "  🐛 Created bug: ${bugKey}"
        } else {
            echo "  [DryRun] Would create bug: ${summary}"
            bugKey = 'DRY-RUN'
        }
    }

    // ── Поиск тест-кейса — возвращает String key или null ─────────────────
    String tcKey = dryRun ? null
        : zephyrClient.findTestCaseByName(config.projectKey, test.testName)

    if (tcKey) {
        echo "  🔗 Linking bug ${bugKey} to test case: ${tcKey}"
        if (!dryRun) {
            zephyrClient.linkIssueToTestCase(tcKey, bugKey)
            jiraClient.linkIssues(config.jiraBaseUrl, bugKey, tcKey)
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