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
