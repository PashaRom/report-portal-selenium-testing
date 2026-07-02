def call(String status, String branch = 'unknown', String commit = 'unknown', String author = 'unknown') {

    def message = """${status}

    Job: ${env.JOB_NAME}
    Build: #${env.BUILD_NUMBER}
    Branch: ${branch}
    Commit: ${commit}
    Author: ${author}
    ${env.BUILD_URL}""".trim()

        echo """
    ================ Teams Notification ================
    ${message}
    ===================================================
    """

        withCredentials([string(credentialsId: 'TEAMS_WEBHOOK', variable: 'WEBHOOK')]) {
            withEnv(["MSG=${message}"]) {
                sh '''
                    curl -s -X POST "$WEBHOOK" \
                        -H "Content-Type: application/json" \
                        -d "{\\"text\\":\\"$MSG\\"}"
                '''
        }
    }
}