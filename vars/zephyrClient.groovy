private static final String ZEPHYR_BASE = 'https://api.zephyrscale.smartbear.com/v2'

def uploadResults(String token, String projectKey, String filePath) {
    sh("""curl -s -X POST \
        -H "Authorization: Bearer ${token}" \
        -F "file=@${filePath}" \
        "${ZEPHYR_BASE}/automations/executions/junit?projectKey=${projectKey}" """)
}

def findTestCaseByName(String token, String projectKey, String testName) {
    def encoded  = URLEncoder.encode(testName, 'UTF-8')
    def response = sh(
        script: """curl -s -X GET \
            -H "Authorization: Bearer ${token}" \
            "${ZEPHYR_BASE}/testcases?projectKey=${projectKey}&text=${encoded}&maxResults=1" """,
        returnStdout: true
    ).trim()

    def json = readJSON text: response
    return (json.values && json.values.size() > 0) ? json.values[0] : null
}

def linkIssueToTestCase(String token, String testCaseKey, String issueKey) {
    sh("""curl -s -X POST \
        -H "Authorization: Bearer ${token}" \
        -H "Content-Type: application/json" \
        -d '{"issueKey":"${issueKey}"}' \
        "${ZEPHYR_BASE}/testcases/${testCaseKey}/links/issues" """)
}