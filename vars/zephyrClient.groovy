import groovy.json.JsonSlurper

def uploadResults(String projectKey, String filePath) {
    def zephyrBase = 'https://api.zephyrscale.smartbear.com/v2'
    sh('''curl -s -X POST \
        -H "Authorization: Bearer ${ZEPHYR_TOKEN}" \
        -F "file=@''' + filePath + '''" \
        "''' + zephyrBase + '''/automations/executions/junit?projectKey=''' + projectKey + '''"''')
}

def findTestCaseByName(String projectKey, String testName) {
    def zephyrBase = 'https://api.zephyrscale.smartbear.com/v2'

    // Prefer deterministic mapping from test name prefix, e.g. KAN_T2_* -> KAN-T2.
    def directKey = extractTestCaseKeyFromTestName(testName)
    if (directKey) {
        echo "  [DEBUG] Zephyr direct test case key candidate: ${directKey}"
        def directResponse = sh(
            script: '''curl -s -o .zephyr_tc_get.json -w "%{http_code}" -X GET \
                -H "Authorization: Bearer ${ZEPHYR_TOKEN}" \
                "''' + zephyrBase + '''/testcases/''' + directKey + '''"''',
            returnStdout: true
        ).trim()

        def statusCode = directResponse
        def body = readFile('.zephyr_tc_get.json').trim()
        sh 'rm -f .zephyr_tc_get.json'

        if (statusCode == '200') {
            def directJson = new JsonSlurper().parseText(body)
            if ((directJson?.key as String)?.equalsIgnoreCase(directKey)) {
                echo "  [DEBUG] Zephyr resolved test case by key: ${directJson.key}"
                return directJson.key as String
            }
        }
        echo "  [DEBUG] Zephyr direct key lookup failed for ${directKey}, fallback to text search"
    }

    def encoded    = URLEncoder.encode(testName, 'UTF-8')

    def response = sh(
        script: '''curl -s -X GET \
            -H "Authorization: Bearer ${ZEPHYR_TOKEN}" \
            "''' + zephyrBase + '''/testcases?projectKey=''' + projectKey + '''&text=''' + encoded + '''&maxResults=1"''',
        returnStdout: true
    ).trim()

    def json = new JsonSlurper().parseText(response)
    if (json.values && json.values.size() > 0) {
        def expectedKey = extractTestCaseKeyFromTestName(testName)
        if (expectedKey) {
            def exact = json.values.find { tc ->
                (tc?.key as String)?.equalsIgnoreCase(expectedKey)
            }
            if (exact) {
                echo "  [DEBUG] Zephyr resolved test case by exact search key: ${exact.key}"
                return exact.key as String
            }
        }

        echo "  [DEBUG] Zephyr resolved test case by first search result: ${json.values[0].key}"
        return json.values[0].key as String
    }
    return null
}

private def extractTestCaseKeyFromTestName(String testName) {
    if (!testName) {
        return null
    }

    def matcher = (testName =~ /^([A-Za-z][A-Za-z0-9]+)_T(\d+)_/)
    if (matcher.matches()) {
        def project = matcher[0][1].toUpperCase()
        def number = matcher[0][2]
        return "${project}-T${number}"
    }

    return null
}

def linkIssueToTestCase(String testCaseKey, String issueId) {
    // Zephyr требует числовой issueId, не issueKey
    def zephyrBase  = 'https://api.zephyrscale.smartbear.com/v2'
    def payloadFile = '.zephyr_link.json'

    writeFile file: payloadFile, text: '{"issueId":' + issueId + '}'

    def response = sh(
        script: '''curl -s -X POST \
            -H "Authorization: Bearer ${ZEPHYR_TOKEN}" \
            -H "Content-Type: application/json" \
            -d @''' + payloadFile + ''' \
            "''' + zephyrBase + '''/testcases/''' + testCaseKey + '''/links/issues"''',
        returnStdout: true
    ).trim()

    sh 'rm -f ' + payloadFile

    if (response) {
        echo "  [DEBUG] zephyr linkIssue response: ${response}"
    }
}