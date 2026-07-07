/**
 * Парсит junit.xml через python3
 * @param filePath — путь к файлу на агенте
 * Возвращает List<Map> упавших тестов
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

    // Сохраняем python скрипт во временный файл — избегаем проблем с кавычками
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