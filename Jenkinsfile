pipeline {
    agent any

    
environment {
        PROJECT_DIR = 'SeleniumFrameworkInteraction'
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'module-8.1',
                    url: 'https://github.com/PashaRom/report-portal-selenium-testing.git'
            }
        }

        stage('Check .NET') {
            steps {
                sh 'dotnet --version'
            }
        }

        stage('Restore') {
            steps {
                dir("${env.PROJECT_DIR}") {
                    sh 'dotnet restore'
                }
            }
        }

        stage('Build') {
            steps {
                dir("${env.PROJECT_DIR}") {
                    sh 'dotnet build --no-restore'
                }
            }
        }
    }
}