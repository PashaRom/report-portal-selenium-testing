/**
 * Jira REST API Client
 * Tokens are read directly from the environment (set via withCredentials in zephyrUploader)
 */

def findOpenBugBySummary(String baseUrl, String projectKey, String summary) {
    def escapedSummary = summary.replace('"', '\\"')
    def jql = 'project = "' + projectKey + '" ' +
              'AND issuetype = Bug ' +
              'AND status != Done ' +
              'AND status != Closed ' +
              'AND status != Resolved ' +
              'AND summary ~ "' + escapedSummary + '"'

    def encoded  = URLEncoder.encode(jql, 'UTF-8')
    def url      = baseUrl + '/rest/api/3/search?jql=' + encoded + '&maxResults=1&fields=summary,status'

    def response = sh(
        script: '''curl -s -X GET \
            -H "Authorization: Bearer ${JIRA_TOKEN}" \
            -H "Content-Type: application/json" \
            "''' + url + '''"''',
        returnStdout: true
    ).trim()

    def json = readJSON text: response
    return json.total > 0 ? json.issues[0] : null
}

def createBug(String baseUrl, String projectKey, String summary, String description) {
    // Pass the payload through a file — avoids issues with quotes in sh
    def payload = [
        fields: [
            project    : [key: projectKey],
            summary    : summary,
            description: [
                type   : 'doc',
                version: 1,
                content: [[
                    type   : 'paragraph',
                    content: [[type: 'text', text: description]]
                ]]
            ],
            issuetype  : [name: 'Bug'],
            priority   : [name: 'High']
        ]
    ]

    def payloadFile = '.jira_create_bug_payload.json'
    writeJSON file: payloadFile, json: payload

    def response = sh(
        script: '''curl -s -X POST \
            -H "Authorization: Bearer ${JIRA_TOKEN}" \
            -H "Content-Type: application/json" \
            -d @''' + payloadFile + ''' \
            "''' + baseUrl + '''/rest/api/3/issue"''',
        returnStdout: true
    ).trim()

    sh "rm -f ${payloadFile}"

    return readJSON text: response
}

def linkIssues(String baseUrl, String bugKey, String testCaseKey) {
    def payload = [
        type        : [name: 'relates to'],
        inwardIssue : [key: bugKey],
        outwardIssue: [key: testCaseKey]
    ]

    def payloadFile = '.jira_link_payload.json'
    writeJSON file: payloadFile, json: payload

    sh('''curl -s -X POST \
        -H "Authorization: Bearer ${JIRA_TOKEN}" \
        -H "Content-Type: application/json" \
        -d @''' + payloadFile + ''' \
        "''' + baseUrl + '''/rest/api/3/issueLink"''')

    sh "rm -f ${payloadFile}"
}