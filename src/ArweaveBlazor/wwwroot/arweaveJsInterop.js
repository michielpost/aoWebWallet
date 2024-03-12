// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.
import {
    message,
    result,
    results,
    dryrun,
    createDataItemSigner
} from "https://www.unpkg.com/@permaweb/aoconnect@0.0.45/dist/browser.js";

let arweave;

export function loadJs(sourceUrl) {
    if (sourceUrl.Length == 0) {
        console.error("Invalid source URL");
        return;
    }

    var tag = document.createElement('script');
    tag.src = sourceUrl;
    tag.type = "text/javascript";

    //tag.onload = function () {
    //    console.log("Script loaded successfully");
    //}

    tag.onerror = function () {
        console.error("Failed to load script");
    }

    document.body.appendChild(tag);
}

export async function InitArweave() {
    arweave = Arweave.init({});
}

export async function HasArConnect() {
    if (window.arweaveWallet) {
        return true;
    }
    else {
        return false;
    }
};

export async function GetWalletBalance(address) {
    var result = await arweave.wallets.getBalance(address)
    console.log(result);
    
    return result;
}

export async function Connect(permissions, appInfo) {
    var result = await window.arweaveWallet.connect(permissions, appInfo)
    console.log(result);

    return result;
}

export async function Disconnect() {
    var result = await window.arweaveWallet.disconnect()
    console.log(result);
    return result;
}

export async function GetActiveAddress(permissions, appInfo) {
    var result = await window.arweaveWallet.getActiveAddress()
    console.log(result);

    return result;
}

export async function Send(processId, data, tags) {
    //let tags = [
    //    { name: "Your-Tag-Name-Here", value: "your-tag-value" },
    //    { name: "Another-Tag", value: "another-value" },
    //];
    let signer = createDataItemSigner(window.arweaveWallet);

    try {
        let result = await message({
            process: processId,
            tags: tags,
            signer: signer,
            data: data,
        });

        console.log(result);
        return result;
    } catch (error) {
        console.error(error);
    }

}


export async function SendDryRun(processId, data, tags) {
    //let tags = [
    //    { name: "Your-Tag-Name-Here", value: "your-tag-value" },
    //    { name: "Another-Tag", value: "another-value" },
    //];
    //let signer = createDataItemSigner(window.arweaveWallet);

    try {
        let { Messages, Spawns, Output, Error } = await dryrun({
            process: processId,
            tags: tags,
            //signer: signer,
            data: data,
        });

        console.log(Messages);
        if (Messages.length > 0) {
            return Messages[0].Data;
        }
        return null;

    } catch (error) {
        console.error(error);
    }

}


export async function GetResult(processId, msgId) {
    let { Messages, Spawns, Output, Error } = await result({
        // the arweave TXID of the message
        message: msgId,
        // the arweave TXID of the process
        process: processId,
    });

    console.log(Messages);

    if (Messages.length > 0) {
        return Messages[0].Data;
    }
    return null;
}

export async function GetResults(processId, limit) {
    console.log("test getting results");
    // fetching the first page of results
    let resultsOut = await results({
        process: processId,
        sort: "ASC",
        limit: limit,
    });

    console.log(resultsOut);
}

