def call(String status) {

    def branch = env.BUILD_BRANCH
        ?: env.GIT_BRANCH
        ?: sh(script: 'git rev-parse --abbrev-ref HEAD', returnStdout: true).trim()

    def commit = sh(
        script: 'git rev-parse --short HEAD',
        returnStdout: true
    ).trim()

    def author = env.GIT_AUTHOR_NAME
        ?: sh(script: 'git log -1 --pretty=format:"%an"', returnStdout: true).trim()

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