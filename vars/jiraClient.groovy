/**
 * Jira REST API Client - No Secret Interpolation
 */

def findOpenBugBySummary(String baseUrl, String token, String projectKey, String summary) {
    def escapedSummary = summary.replace('"', '\\"')
    def jql = 'project = "' + projectKey + '" ' +
              'AND issuetype = Bug ' +
              'AND status != Done ' +
              'AND status != Closed ' +
              'AND status != Resolved ' +
              'AND summary ~ "' + escapedSummary + '"'

    def encoded  = URLEncoder.encode(jql, 'UTF-8')
    def url      = "${baseUrl}/rest/api/3/search?jql=${encoded}&maxResults=1&fields=summary,status"
    def response

    withEnv(["JIRA_TOKEN=${token}"]) {
        response = sh(
            script: '''curl -s -X GET \
                -H "Authorization: Bearer ${JIRA_TOKEN}" \
                -H "Content-Type: application/json" \
                "''' + url + '''"''',
            returnStdout: true
        ).trim()
    }

    def json = readJSON text: response
    return json.total > 0 ? json.issues[0] : null
}

def createBug(String baseUrl, String token, String projectKey,
              String summary, String description) {
    def safeSummary     = summary.replace("'", "'\\''").replace('"', '\\"')
    def safeDescription = description.replace("'", "'\\''").replace('"', '\\"')

    def payload = """{
        "fields": {
            "project":     { "key": "${projectKey}" },
            "summary":     "${safeSummary}",
            "description": {
                "type": "doc", "version": 1,
                "content": [{
                    "type": "paragraph",
                    "content": [{ "type": "text", "text": "${safeDescription}" }]
                }]
            },
            "issuetype": { "name": "Bug" },
            "priority":  { "name": "High" }
        }
    }"""

    def response
    withEnv(["JIRA_TOKEN=${token}", "JIRA_PAYLOAD=${payload}"]) {
        response = sh(
            script: '''curl -s -X POST \
                -H "Authorization: Bearer ${JIRA_TOKEN}" \
                -H "Content-Type: application/json" \
                -d "${JIRA_PAYLOAD}" \
                "''' + baseUrl + '''/rest/api/3/issue"''',
            returnStdout: true
        ).trim()
    }

    return readJSON text: response
}

def linkIssues(String baseUrl, String token, String bugKey, String testCaseKey) {
    def payload = '{"type":{"name":"relates to"},"inwardIssue":{"key":"' + bugKey + '"},"outwardIssue":{"key":"' + testCaseKey + '"}}'

    withEnv(["JIRA_TOKEN=${token}", "JIRA_PAYLOAD=${payload}"]) {
        sh '''curl -s -X POST \
            -H "Authorization: Bearer ${JIRA_TOKEN}" \
            -H "Content-Type: application/json" \
            -d "${JIRA_PAYLOAD}" \
            "''' + baseUrl + '''/rest/api/3/issueLink"'''
    }
}