let quill;

window.initQuill = (editorId) => {
    quill = new Quill(`#${editorId}`, {
        theme: "snow",
        modules: {
            toolbar: [
                ["bold", "italic", "underline"],
                [{ list: "ordered" }, { list:  "bullet" }],
                [{ header: [1, 2, false] }],
                ["clean"]
            ]
        }
    });
};

window.getQuillHtml = () => {
    return quill.root.innerHTML;
};

window.setQuillHtml = (html) => {
    quill.root.innerHTML = html;
};

window.getQuillText = () => {
    return quill.getText();
};

window.clearQuill = () => {
    quill.setText('');
};