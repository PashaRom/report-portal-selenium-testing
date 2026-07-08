import groovy.json.JsonOutput
import groovy.json.JsonSlurper

/**
 * JIRA_CLOUD_TOKEN формат: "email@company.com:your_api_token"
 * В Jenkins credentials: Secret text = "pasharomash@gmail.com:ATATxxx..."
 */

def findOpenBugBySummary(String baseUrl, String projectKey, String summary) {
    def escapedSummary = summary.replace('"', '\\"')
    def jql = 'project = "' + projectKey + '" ' +
              'AND issuetype = Bug ' +
              'AND status != Done ' +
              'AND status != Closed ' +
              'AND status != Resolved ' +
              'AND summary ~ "' + escapedSummary + '"'

    def payload = JsonOutput.toJson([
        jql       : jql,
        maxResults: 1,
        fields    : ['summary', 'status', 'id']
    ])

    def payloadFile = '.jira_search.json'
    writeFile file: payloadFile, text: payload

    def response = sh(
        script: '''curl -s -X POST \
            -u "pasharomash@gmail.com:$JIRA_CLOUD_TOKEN" \
            -H "Accept: application/json" \
            -H "Content-Type: application/json" \
            -d @${payloadFile} \
            "''' + baseUrl + '''/rest/api/3/search/jql"''',
        returnStdout: true
    ).trim()

    sh 'rm -f ' + payloadFile
    echo "  [DEBUG] findOpenBug response: ${response}"

    def json = new JsonSlurper().parseText(response)
    if (json.issues && json.issues.size() > 0) {
        return json.issues[0].key as String
    }
    return null
}

def createBug(String baseUrl, String projectKey, String summary, String description) {
    def payload = JsonOutput.toJson([
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
    ])

    def payloadFile = '.jira_create_bug.json'
    writeFile file: payloadFile, text: payload

    def response = sh(
        script: '''curl -s -X POST \
            -u "pasharomash@gmail.com:$JIRA_CLOUD_TOKEN" \
            -H "Accept: application/json" \
            -H "Content-Type: application/json" \
            -d @${payloadFile} \
            "''' + baseUrl + '''/rest/api/3/issue"''',
        returnStdout: true
    ).trim()

    sh 'rm -f ' + payloadFile
    echo "  [DEBUG] createBug response: ${response}"

    def json = new JsonSlurper().parseText(response)
    if (json.key) {
        return json.key as String
    }
    echo "  ❌ createBug failed: ${response}"
    return null
}

def getIssueId(String baseUrl, String issueKey) {
    def response = sh(
        script: '''curl -s -X GET \
            -u "pasharomash@gmail.com:$JIRA_CLOUD_TOKEN" \
            -H "Accept: application/json" \
            "''' + baseUrl + '''/rest/api/3/issue/''' + issueKey + '''?fields=id"''',
        returnStdout: true
    ).trim()

    def json = new JsonSlurper().parseText(response)
    return json.id as String
}

def linkIssues(String baseUrl, String bugKey, String testCaseKey) {
    def payload = JsonOutput.toJson([
        type        : [name: 'relates to'],
        inwardIssue : [key: bugKey],
        outwardIssue: [key: testCaseKey]
    ])

    def payloadFile = '.jira_link.json'
    writeFile file: payloadFile, text: payload

    def response = sh(
        script: '''curl -s -X POST \
            -u "pasharomash@gmail.com:$JIRA_CLOUD_TOKEN" \
            -H "Accept: application/json" \
            -H "Content-Type: application/json" \
            -d @${payloadFile} \
            "''' + baseUrl + '''/rest/api/3/issueLink"''',
        returnStdout: true
    ).trim()

    sh 'rm -f ' + payloadFile
    if (response) {
        echo "  [DEBUG] linkIssues response: ${response}"
    }
}