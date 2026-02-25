const fs = require('fs');
const path = require('path');

const featuresDir = 'C:/Users/admin/source/repos/3MEligibility/Angular/src/app/features';

function walkDir(dir, callback) {
    fs.readdirSync(dir).forEach(f => {
        let dirPath = path.join(dir, f);
        let isDirectory = fs.statSync(dirPath).isDirectory();
        isDirectory ? walkDir(dirPath, callback) : callback(path.join(dir, f));
    });
}

walkDir(featuresDir, (filePath) => {
    if (filePath.endsWith('.html')) {
        let content = fs.readFileSync(filePath, 'utf8');
        let newContent = content;

        // Remove legacy classes
        newContent = newContent.replace(/\sclass="cellStyling"/g, '');
        newContent = newContent.replace(/\sclass="tableHeader"/g, '');
        newContent = newContent.replace(/\sclass="bgColorWhite"/g, '');

        // Remove mat-elevation-z8 from tables if they are in a table-wrapper
        // This is a bit safer to do generally if the table-wrapper handles the shadow/border
        // (The user's reference image shows clear card-like borders)
        newContent = newContent.replace(/<table([^>]+)class="([^"]*)mat-elevation-z8([^"]*)"/g, (match, p1, p2, p3) => {
            return `<table${p1}class="${p2}${p3}"`.replace(/\sclass="\s*"/, '');
        });

        // Ensure mat-mdc-table is present if mat-table is present
        // (Optional, but helps with specificity if needed, though global styles handle both)
        // newContent = newContent.replace(/<table([^>]+)mat-table(?![^>]*mat-mdc-table)/g, '<table$1mat-table mat-mdc-table');

        if (content !== newContent) {
            fs.writeFileSync(filePath, newContent, 'utf8');
            console.log(`Cleaned: ${filePath}`);
        }
    }
});
