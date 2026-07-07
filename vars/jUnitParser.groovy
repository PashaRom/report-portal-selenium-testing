def parseFailedTests(String xmlContent) {
    def result = []
    def xml = new XmlSlurper().parseText(xmlContent)
    
    def suites = xml.name() == 'testsuites' ? xml.testsuite : [xml]

    suites.each { suite ->
        suite.testcase.each { testcase ->
            def failure = testcase.failure
            def error   = testcase.error

            if (failure.size() > 0 || error.size() > 0) {
                def node = failure.size() > 0 ? failure : error
                result << [
                    className  : testcase.@classname.text(),
                    testName   : testcase.@name.text(),
                    message    : node.@message.text(),
                    stackTrace : node.text(),
                    type       : node.@type.text()
                ]
            }
        }
    }
    return result
}