// from https://github.com/thinktecture-labs/article-blazor-sqlite-efcore
export function mountDatabaseInIndexDB() {
    FS.mkdir("/database");
    FS.mount(IDBFS, {}, "/database");
    return syncDatabase(true);
}

export function syncDatabase(populate) {
    return new Promise((resolve, reject) => {
        FS.syncfs(populate, (err) => {
            if (err) {
                console.error(err);
                reject(err);
            } else {
                console.log("Sync successful");
                resolve();
            }
        });
    });
}

export function statDatabaseFile(file) {
    return FS.stat(file);
}

export function readDatabaseFile(file) {
    console.log(FS.stat(file));
    return FS.readFile(file);
}

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}
