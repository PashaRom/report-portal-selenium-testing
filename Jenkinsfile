pipeline {
    agent any

    
    parameters {
        string(
            name: 'TEST_FILTER',
            defaultValue: 'FullyQualifiedName~Cancel_ClosesDialog',
            description: 'dotnet test filter (например: TestCategory=dashboard_crud)'
        )
    }


    
    environment {
            BASE_URL = 'http://192.168.1.4:8080/'
            PROJECT_DIR = 'SeleniumFrameworkInteraction'
            REMOTE_URL = 'http://192.168.56.1:4444/wd/hub'
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
                dir("${env.PROJECT_DIR}/UITests") {
                    sh """
                    mkdir -p ${env.ALLURE_RESULTS}

                    BROWSERS=${env.BROWSERS} \
                    BaseUrl=${env.BASE_URL} \
                    DriverSettings__Headless=true \
                    DriverSettings__Remote=true \
                    dotnet test --no-build --logger "trx" \
                        --results-directory ${env.ALLURE_RESULTS} \
                        --filter "${params.TEST_FILTER}"
                    """
                }
            }
        }

        
        
        stage('Publish Allure') {
            steps {
                allure([
                        results: [[path: "${env.PROJECT_DIR}/UITests/${env.ALLURE_RESULTS}"]]
                ])
            }
        }


    }
    post {
        always {
            archiveArtifacts artifacts: '**/allure-results/**'
        }
    }
}
