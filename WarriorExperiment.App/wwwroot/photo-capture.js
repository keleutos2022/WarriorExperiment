// Photo capture and file selection functions
window.capturePhoto = function() {
    console.log('capturePhoto called');
    return new Promise((resolve, reject) => {
        // Create file input for camera capture
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';
        input.capture = 'environment'; // Use back camera on mobile
        
        input.onchange = function(event) {
            const file = event.target.files[0];
            if (file) {
                processImageFile(file, resolve, reject);
            } else {
                resolve(null);
            }
        };
        
        input.oncancel = function() {
            resolve(null);
        };
        
        // Trigger file picker
        input.click();
    });
};

window.selectFile = function() {
    console.log('selectFile called');
    return new Promise((resolve, reject) => {
        // Create file input for file selection
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';
        
        input.onchange = function(event) {
            const file = event.target.files[0];
            if (file) {
                processImageFile(file, resolve, reject);
            } else {
                resolve(null);
            }
        };
        
        input.oncancel = function() {
            resolve(null);
        };
        
        // Trigger file picker
        input.click();
    });
};

function processImageFile(file, resolve, reject) {
    // Validate file type
    if (!file.type.startsWith('image/')) {
        reject(new Error('Please select a valid image file'));
        return;
    }
    
    // Validate file size (max 20MB)
    const maxSize = 20 * 1024 * 1024;
    if (file.size > maxSize) {
        reject(new Error('Image file is too large. Please select an image smaller than 20MB.'));
        return;
    }
    
    const reader = new FileReader();
    
    reader.onload = function(e) {
        const img = new Image();
        img.onload = function() {
            // Compress and resize image
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            
            // Get original dimensions
            let { width, height } = img;
            
            // Calculate new dimensions for ~3 megapixels (roughly 1732x1732 for square or proportional)
            const targetMegapixels = 3000000; // 3 megapixels
            const currentPixels = width * height;
            
            let maxDimension;
            if (currentPixels > targetMegapixels) {
                const ratio = Math.sqrt(targetMegapixels / currentPixels);
                maxDimension = Math.max(width, height) * ratio;
            } else {
                maxDimension = Math.max(width, height); // Keep original size if smaller
            }
            
            if (width > height) {
                if (width > maxDimension) {
                    height = (height * maxDimension) / width;
                    width = maxDimension;
                }
            } else {
                if (height > maxDimension) {
                    width = (width * maxDimension) / height;
                    height = maxDimension;
                }
            }
            
            canvas.width = width;
            canvas.height = height;
            
            // Draw and compress
            ctx.drawImage(img, 0, 0, width, height);
            
            // Convert canvas to blob with higher quality for 3 megapixels
            canvas.toBlob(function(blob) {
                const reader = new FileReader();
                reader.onload = function() {
                    const base64Data = reader.result.split(',')[1];
                    console.log('Image processed, base64 length:', base64Data.length);
                    console.log('Final dimensions:', width, 'x', height, '=', width * height, 'pixels');
                    resolve(base64Data);
                };
                reader.readAsDataURL(blob);
            }, 'image/jpeg', 0.85); // Higher quality for 3MP
        };
        
        img.onerror = function() {
            reject(new Error('Failed to process image'));
        };
        
        img.src = e.target.result;
    };
    
    reader.onerror = function() {
        reject(new Error('Failed to read file'));
    };
    
    reader.readAsDataURL(file);
}

// New functions for direct file input approach
window.triggerFileInput = function(inputElement) {
    inputElement.click();
};

window.processSelectedFile = function(inputElementId) {
    return new Promise((resolve, reject) => {
        const input = document.getElementById(inputElementId);
        if (!input || !input.files || input.files.length === 0) {
            resolve(null);
            return;
        }
        
        const file = input.files[0];
        processImageFile(file, resolve, reject);
        
        // Clear the input so the same file can be selected again
        input.value = '';
    });
};