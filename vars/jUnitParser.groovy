/**
 * Parses junit.xml via python3
 * @param filePath — path to the file on the agent
 * @return List<Map> of failed tests
 */

def parseFailedTests(String filePath) {
    def result = []

    def pythonScript = '''
import sys
import xml.etree.ElementTree as ET

path = sys.argv[1]
tree = ET.parse(path)
root = tree.getroot()

if root.tag == "testsuites":
    suites = root.findall("testsuite")
else:
    suites = [root]

SEP = "|||"
BLOCK = "---END---"

for suite in suites:
    for tc in suite.findall("testcase"):
        failure_node = tc.find("failure")
        error_node   = tc.find("error")

        # ✅ Explicit check using is not None
        if failure_node is not None:
            node = failure_node
        elif error_node is not None:
            node = error_node
        else:
            continue

        classname  = tc.get("classname", "")
        testname   = tc.get("name", "")
        err_type   = node.get("type", "")
        message    = node.get("message", "").replace("\\n", " ").replace("|||", " ")
        stacktrace = (node.text or "").strip().replace("\\n", "\\\\n").replace("|||", " ")

        print(classname + SEP + testname + SEP + err_type + SEP + message + SEP + stacktrace)
        print(BLOCK)
'''

    def scriptFile = '.junitparser_tmp.py'
    writeFile file: scriptFile, text: pythonScript

    def rawOutput = sh(
        script: "python3 ${scriptFile} '${filePath}'",
        returnStdout: true
    ).trim()

    sh "rm -f ${scriptFile}"

    if (!rawOutput) {
        return result
    }

    rawOutput.split('---END---').each { block ->
        def line = block.trim()
        if (!line) return

        def parts = line.split('\\|\\|\\|', -1)
        if (parts.size() >= 5) {
            result << [
                className  : parts[0].trim(),
                testName   : parts[1].trim(),
                type       : parts[2].trim(),
                message    : parts[3].trim(),
                stackTrace : parts[4].trim().replace('\\n', '\n')
            ]
        }
    }

    return result
}