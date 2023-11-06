window.isMouseOver = function(id) {
    return document.getElementById(id).matches(':hover');
}

window.focusElementById = (id) => {
    const element = document.getElementById(id);
    if (element) {
        element.focus();
    }
}

window.setCursorPosition = (reference, position) => {
    let range = document.createRange();
    let sel = window.getSelection();
    range.setStart(reference.childNodes[0], position);
    range.collapse(true);
    sel.removeAllRanges();
    sel.addRange(range);
}

window.getCursorPosition = (reference) => {
    let selection = window.getSelection();
    let range = selection.rangeCount > 0 ? selection.getRangeAt(0) : document.createRange();
    let preCaretRange = range.cloneRange();
    preCaretRange.selectNodeContents(reference);
    preCaretRange.setEnd(range.endContainer, range.endOffset);
    return preCaretRange.toString().length;
}

window.getText = (reference) => {
    return reference.innerText;
}

window.removeElement = (element) => {
    element.parentNode.removeChild(element);
};
