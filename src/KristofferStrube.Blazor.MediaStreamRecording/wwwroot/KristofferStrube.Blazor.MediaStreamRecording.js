export function getAttribute(object, attribute) { return object[attribute]; }

export function constructMediaRecorder(stream, options) {
    return new MediaRecorder(stream, options);
}