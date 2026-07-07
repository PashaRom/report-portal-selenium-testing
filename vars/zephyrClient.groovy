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
    def encoded    = URLEncoder.encode(testName, 'UTF-8')

    def response = sh(
        script: '''curl -s -X GET \
            -H "Authorization: Bearer ${ZEPHYR_TOKEN}" \
            "''' + zephyrBase + '''/testcases?projectKey=''' + projectKey + '''&text=''' + encoded + '''&maxResults=1"''',
        returnStdout: true
    ).trim()

    def json = new JsonSlurper().parseText(response)
    return (json.values && json.values.size() > 0) ? json.values[0] : null
}

def linkIssueToTestCase(String testCaseKey, String issueKey) {
    def zephyrBase = 'https://api.zephyrscale.smartbear.com/v2'
    def payload    = '{"issueKey":"' + issueKey + '"}'
    def payloadFile = '.zephyr_link.json'

    writeFile file: payloadFile, text: payload

    sh('''curl -s -X POST \
        -H "Authorization: Bearer ${ZEPHYR_TOKEN}" \
        -H "Content-Type: application/json" \
        -d @''' + payloadFile + ''' \
        "''' + zephyrBase + '''/testcases/''' + testCaseKey + '''/links/issues"''')

    sh 'rm -f ' + payloadFile
}