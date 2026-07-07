def uploadResults(String token, String projectKey, String filePath) {
    def zephyrBase = 'https://api.zephyrscale.smartbear.com/v2'
    sh("""curl -s -X POST \
        -H "Authorization: Bearer ${token}" \
        -F "file=@${filePath}" \
        "${zephyrBase}/automations/executions/junit?projectKey=${projectKey}" """)
}

def findTestCaseByName(String token, String projectKey, String testName) {
    def zephyrBase = 'https://api.zephyrscale.smartbear.com/v2'
    def encoded  = URLEncoder.encode(testName, 'UTF-8')
    def response = sh(
        script: """curl -s -X GET \
            -H "Authorization: Bearer ${token}" \
            "${zephyrBase}/testcases?projectKey=${projectKey}&text=${encoded}&maxResults=1" """,
        returnStdout: true
    ).trim()

    def json = readJSON text: response
    return (json.values && json.values.size() > 0) ? json.values[0] : null
}

def linkIssueToTestCase(String token, String testCaseKey, String issueKey) {
    def zephyrBase = 'https://api.zephyrscale.smartbear.com/v2'
    sh("""curl -s -X POST \
        -H "Authorization: Bearer ${token}" \
        -H "Content-Type: application/json" \
        -d '{"issueKey":"${issueKey}"}' \
        "${zephyrBase}/testcases/${testCaseKey}/links/issues" """)
}