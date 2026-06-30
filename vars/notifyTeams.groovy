def call(String status) {
    def message = """
        ${status}

        Job: ${env.JOB_NAME}
        Build: #${env.BUILD_NUMBER}
        Branch: ${env.BRANCH_NAME}
        Commit: ${env.GIT_COMMIT.take(7)}
        Author: ${env.GIT_AUTHOR_NAME}
        ${env.BUILD_URL}
    """.trim()

    echo """
================ Teams Notification ================
${message}
===================================================
"""

    withCredentials([string(credentialsId: 'TEAMS_WEBHOOK', variable: 'WEBHOOK')]) {
        sh """
            curl -s -X POST "$WEBHOOK" \
                 -H "Content-Type: application/json" \
                 -d '{
                       "text":"${message}"
                     }'
        """
    }
}