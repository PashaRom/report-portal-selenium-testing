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
        
        choice(
            name: 'HEADLESS_MODE',
            choices: ['HEADLESS', 'NON_HEADLESS'],
            description: 'Browser mode'
        )

        string(
            name: 'THREADS',
            defaultValue: '3',
            description: 'Number of parallel test threads'
        )

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
            ALLURE_DIR = 'allure-results'
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

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    dir("${env.PROJECT_DIR}") {
                        withCredentials([string(credentialsId: 'SONAR_TOKEN', variable: 'SONAR_TOKEN')]) {
                            sh '''                            
                            
                            dotnet new tool-manifest || true
                            dotnet tool install dotnet-sonarscanner || true
                            dotnet tool restore

                            dotnet tool run dotnet-sonarscanner begin \
                                /k:"selenium-framework" \
                                /d:sonar.login="$SONAR_TOKEN" \
                                /n:"Selenium Framework"

                            dotnet build --no-restore

                            dotnet tool run dotnet-sonarscanner end \
                                /d:sonar.login="$SONAR_TOKEN"

                            '''

                        }
                    }
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
                                            BROWSERS=${b} \\
                                            BaseUrl=${env.BASE_URL} \\
                                            DriverSettings__Remote=true \\
                                            DriverSettings__Headless=${params.HEADLESS_MODE == 'HEADLESS'} \\
                                            ReportPortal__Launch__Name="RP UI NUnit - ${b}" \\
                                            REPORTPORTAL_SERVER_APIKEY=\$RP_KEY \\
                                            dotnet test --no-build \\
                                                --results-directory ${env.ALLURE_DIR}/${b} \\
                                                --filter "${params.TEST_FILTER}" \\
                                                -- NUnit.NumberOfTestWorkers=${params.THREADS}
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
                            [path: "${env.PROJECT_DIR}/UITests/bin/Debug/net8.0/allure-results"]
                        ]
                    ])
                }
            }
        }

    }
    post {
        always {
            archiveArtifacts artifacts: 'SeleniumFrameworkInteraction/UITests/bin/Debug/net8.0/allure-results/**'
        }
    }
}
