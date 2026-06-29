def call() {
    stage('Check .NET') {
        sh 'dotnet --version'
    }
}