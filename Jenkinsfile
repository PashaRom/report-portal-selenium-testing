pipeline {
    agent any

    
    environment {
            PROJECT_DIR = 'SeleniumFrameworkInteraction'
            REMOTE_URL = 'http://host.docker.internal:4444/wd/hub'
            BROWSERS = 'Chrome'
            ALLURE_RESULTS = 'allure-results'
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

        
        stage('Test (Selenoid)') {
            steps {
                dir("${PROJECT_DIR}/UITests") {
                    sh """
                    mkdir -p ${ALLURE_RESULTS}

                    BROWSERS=${BROWSERS} DriverSettings__Headless=true DriverSettings__Remote=true dotnet test --no-build --logger "trx" --results-directory ${ALLURE_RESULTS}
                    """
                }
            }
        }

        
        stage('Publish Allure') {
            steps {
                allure includeProperties: false,
                    jdk: '',
                    results: [[path: "${PROJECT_DIR}/UITests/${ALLURE_RESULTS}"]]
            }
        }
    }
    post {
        always {
            archiveArtifacts artifacts: '**/allure-results/**'
        }
    }
}
