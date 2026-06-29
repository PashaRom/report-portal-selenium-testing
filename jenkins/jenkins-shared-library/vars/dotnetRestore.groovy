def call(String projectDir) {
    stage('Restore') {
            dir(projectDir) {
                sh 'dotnet restore'
            }

    }
}