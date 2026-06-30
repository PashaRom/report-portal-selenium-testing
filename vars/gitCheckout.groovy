def call(String branch, String url) {
    stage('Checkout') {
        git branch: branch,
                url: url
    }
}