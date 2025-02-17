const normalize = (strArray) => {
    var resultArray = [];
    if (strArray.length === 0) { return ''; }

    if (typeof strArray[0] !== 'string') {
        throw new TypeError('Url must be a string. Received ' + strArray[0]);
    }

    // If the first part is a plain protocol, we combine it with the next part.
    if (strArray[0].match(/^[^/:]+:\/*$/) && strArray.length > 1) {
        var first = strArray.shift();
        strArray[0] = first + strArray[0];
    }

    // There must be two or three slashes in the file protocol, two slashes in anything else.
    if (strArray[0].match(/^file:\/\/\//)) {
        strArray[0] = strArray[0].replace(/^([^/:]+):\/*/, '$1:///');
    } else {
        strArray[0] = strArray[0].replace(/^([^/:]+):\/*/, '$1://');
    }

    for (var i = 0; i < strArray.length; i++) {
        var component = strArray[i];

        if (typeof component !== 'string') {
            throw new TypeError('Url must be a string. Received ' + component);
        }

        if (component === '') { continue; }

        if (i > 0) {
            // Removing the starting slashes for each component but the first.
            component = component.replace(/^[\/]+/, '');
        }
        if (i < strArray.length - 1) {
            // Removing the ending slashes for each component but the last.
            component = component.replace(/[\/]+$/, '');
        } else {
            // For the last component we will combine multiple slashes to a single one.
            component = component.replace(/[\/]+$/, '/');
        }

        resultArray.push(component);

    }

    var str = resultArray.join('/');
    // Each input component is now separated by a single slash except the possible first plain protocol part.

    // remove trailing slash before parameters or hash
    str = str.replace(/\/(\?|&|#[^!])/g, '$1');

    // replace ? in parameters with &
    var parts = str.split('?');
    str = parts.shift() + (parts.length > 0 ? '?' : '') + parts.join('&');

    return str;
}

const urlJoin = (...input) => {
    return normalize(input);
}

/**
 * 由于 ThreejsLoaders 并不支持使用相对路径导入资源
 * 因此使用以下函数以根据 import.meta.env.BASE_URL 即 Vite.ResolvedConfig 下的 Base 对资源路径进行拼接
 * @param {*} publicResourceUrl 资源在public目录下的位置
 * @returns
 */
export const baseUrlT = (publicResourceUrl) => {
    return urlJoin(import.meta.env.BASE_URL, publicResourceUrl);
}
