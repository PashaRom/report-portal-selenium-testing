pipeline {
    agent any

    
environment {
        PROJECT_DIR = 'tests/ReportPortal.Tests'
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
                dir('SeleniumFrameworkInteraction') {
                    sh 'dotnet restore'
                }
                dir("${env.PROJECT_DIR}") {
                    sh 'dotnet restore'
                }
            }
        }

        stage('Build') {
            steps {
                dir('SeleniumFrameworkInteraction') {
                    sh 'dotnet build --no-restore'
                }
                dir("${env.PROJECT_DIR}") {
                    sh 'dotnet build --no-restore'
                }
            }
        }
    }
}