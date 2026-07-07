/**
 * Parses junit.xml via Python3 (bypassing Jenkins sandbox limitations)
 * Returns a List<Map> of failed tests
 */

def parseFailedTests(String filePath) {
    def result = []

    def pythonScript = '''
import sys
import xml.etree.ElementTree as ET

path = sys.argv[1]
tree = ET.parse(path)
root = tree.getroot()

# Support for <testsuites> and <testsuite>
if root.tag == "testsuites":
    suites = root.findall("testsuite")
else:
    suites = [root]

SEP = "|||"
BLOCK = "---END---"

for suite in suites:
    for tc in suite.findall("testcase"):
        node = tc.find("failure") or tc.find("error")
        if node is not None:
            classname  = tc.get("classname", "")
            testname   = tc.get("name", "")
            err_type   = node.get("type", "")
            message    = node.get("message", "").replace("\\n", " ")
            stacktrace = (node.text or "").strip().replace("\\n", "\\\\n")
            print(classname + SEP + testname + SEP + err_type + SEP + message + SEP + stacktrace)
            print(BLOCK)
'''

    def rawOutput = sh(
        script: "python3 -c '${pythonScript.replace("'", "'\\''")}' '${filePath}'",
        returnStdout: true
    ).trim()

    if (!rawOutput) {
        return result
    }

    // Parse each block
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