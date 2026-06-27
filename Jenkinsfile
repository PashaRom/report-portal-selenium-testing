pipeline {
    agent any

    
    options {
        buildDiscarder(logRotator(
            numToKeepStr: '10'
        ))
    }

    
    
    parameters {
        booleanParam(name: 'CHROME',  defaultValue: true,  description: 'Run Chrome')
        booleanParam(name: 'EDGE',    defaultValue: false, description: 'Run Edge')
        booleanParam(name: 'FIREFOX', defaultValue: false, description: 'Run Firefox')

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
            ALLURE_RESULTS = 'allure-results'
            REPORT_PORTAL_URL = 'http://192.168.1.4:8080'
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

        stage('Check RP config') {
            steps {
                sh '''
                echo "=== ReportPortal.config.json in output ==="
                cat SeleniumFrameworkInteraction/UITests/bin/Debug/net8.0/ReportPortal.config.json || echo "FILE NOT FOUND"
                
                echo "=== ReportPortal.addins ==="
                cat SeleniumFrameworkInteraction/UITests/bin/Debug/net8.0/ReportPortal.addins || echo "FILE NOT FOUND"
                
                echo "=== ReportPortal DLL ==="
                ls SeleniumFrameworkInteraction/UITests/bin/Debug/net8.0/ReportPortal* || echo "NO RP FILES"
                '''
            }
        }
        
        stage('Test (Selenoid)') {
            steps {
                script {
                    def browsers = []

                    if (params.CHROME)  browsers.add('Chrome')
                    if (params.EDGE)    browsers.add('Edge')
                    if (params.FIREFOX) browsers.add('Firefox')

                    
                    if (browsers.isEmpty()) {
                        error("No browsers selected!")
                    }

                    def parallelStages = [:]
                    
                    for (browser in browsers) {
                        def b = browser
                      
                        parallelStages[b] = {
                            stage("Run on ${b}") {
                                catchError(buildResult: 'SUCCESS', stageResult: 'FAILURE') {
                                    dir("${env.PROJECT_DIR}/UITests") {
                                        withCredentials([string(credentialsId: 'RP_API_KEY', variable: 'RP_KEY')]) {
                                            sh """
                                            mkdir -p ${env.ALLURE_RESULTS}/${b}

                                            BROWSERS=${b} \\
                                            BaseUrl=${env.BASE_URL} \\
                                            DriverSettings__Remote=true \\
                                            REPORTPORTAL_SERVER_APIKEY=\$RP_KEY \\
                                            dotnet test --no-build \\
                                                --results-directory ${env.ALLURE_RESULTS}/${b} \\
                                                --filter "${params.TEST_FILTER}"
                                            """
                                        }
                                    }
                                }
                            }
                        }
                    }

                    parallel parallelStages
                }
            }
        }
        
        stage('Publish Allure') {
            steps {
                script {
                    step([
                        $class: 'AllureReportPublisher',
                        results: [
                            [path: "${env.PROJECT_DIR}/UITests/${env.ALLURE_RESULTS}/Chrome"],
                            [path: "${env.PROJECT_DIR}/UITests/${env.ALLURE_RESULTS}/Firefox"],
                            [path: "${env.PROJECT_DIR}/UITests/${env.ALLURE_RESULTS}/Edge"]
                        ]
                    ])
                }
            }
        }

    }
    post {
        always {
            archiveArtifacts artifacts: '**/allure-results/**'
        }
    }
}
