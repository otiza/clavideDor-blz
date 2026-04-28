window.downloadFileFromBase64 = (fileName, contentType, base64Content) => {
    const link = document.createElement('a');
    link.href = `data:${contentType};base64,${base64Content}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.registerGameKeyListener = (dotNetRef) => {
    window.unregisterGameKeyListener();

    window.__gameKeyHandler = (event) => {
        const activeTag = document.activeElement?.tagName;
        if (activeTag === 'INPUT' || activeTag === 'TEXTAREA' || activeTag === 'SELECT') {
            return;
        }

        const key = (event.key || '').toUpperCase();
        if (key === 'A' || key === 'B' || key === 'C' || key === 'D') {
            dotNetRef.invokeMethodAsync('OnKeyPressed', key);
        }
    };

    window.addEventListener('keydown', window.__gameKeyHandler);
};

window.unregisterGameKeyListener = () => {
    if (!window.__gameKeyHandler) {
        return;
    }

    window.removeEventListener('keydown', window.__gameKeyHandler);
    window.__gameKeyHandler = null;
};
