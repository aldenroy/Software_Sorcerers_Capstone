document.addEventListener("DOMContentLoaded", function () {
    const colorModeSelect = document.getElementById("ColorMode");
    const fontSizeSelect = document.getElementById("FontSize");
    const fontTypeSelect = document.getElementById("FontType");

    // Apply preview changes to the page
    function applyPreviewChanges() {
        // Get the selected values and convert to the format used in CSS
        const colorModeRaw = colorModeSelect.value;
        const fontSizeRaw = fontSizeSelect.value;
        const fontTypeRaw = fontTypeSelect.value;
        
        // Handle color theme
        // Convert to lowercase and handle space in "High Contrast"
        const colorMode = colorModeRaw.toLowerCase().replace(" ", "-");
        
        // Handle font size
        // Convert to lowercase and handle space in "Extra Large"
        const fontSize = fontSizeRaw.toLowerCase().replace(" ", "-");
        
        // Handle font type
        // Convert to lowercase and handle space in "Open Dyslexic"
        const fontType = fontTypeRaw.toLowerCase().replace(" ", "-");

        // Remove all theme classes from both html and body elements
        document.documentElement.classList.remove("light-theme", "dark-theme", "high-contrast-theme");
        document.body.classList.remove("light-theme", "dark-theme", "high-contrast-theme");
        
        // Add appropriate theme class to both html and body elements
        if (colorMode === "light") {
            document.documentElement.classList.add("light-theme");
            document.body.classList.add("light-theme");
        } else if (colorMode === "dark") {
            document.documentElement.classList.add("dark-theme");
            document.body.classList.add("dark-theme");
        } else if (colorMode === "high-contrast") {
            document.documentElement.classList.add("high-contrast-theme");
            document.body.classList.add("high-contrast-theme");
        }

        // Set the data attributes on the HTML element for CSS variable selection
        document.documentElement.setAttribute("data-theme", colorMode);
        document.documentElement.setAttribute("data-font-size", fontSize);
        document.documentElement.setAttribute("data-font-type", fontType);
        
        // Update the logo based on the selected theme
        updateLogo(colorMode);
        
        console.log(`Applied: theme=${colorMode}, font-size=${fontSize}, font-type=${fontType}`);
    }
    
    // Function to update the logo based on the theme
    function updateLogo(colorMode) {
        const logoElement = document.getElementById("logo-main");
        if (logoElement) {
            const basePath = '/images/';
            
            if (colorMode === "dark") {
                logoElement.src = basePath + 'team_logo_dark.png';
                logoElement.alt = "Dark Mode Logo";
            } else if (colorMode === "high-contrast") {
                logoElement.src = basePath + 'team_logo_contrast.png';
                logoElement.alt = "High Contrast Mode Logo";
            } else {
                logoElement.src = basePath + 'team_logo.svg';
                logoElement.alt = "Light Mode Logo";
            }
        }
    }

    // Check if the form elements exist before trying to attach listeners
    if (colorModeSelect && fontSizeSelect && fontTypeSelect) {
        // Initial application of preferences
        applyPreviewChanges();

        // Attach event listeners for changes
        colorModeSelect.addEventListener("change", applyPreviewChanges);
        fontSizeSelect.addEventListener("change", applyPreviewChanges);
        fontTypeSelect.addEventListener("change", applyPreviewChanges);
    } else {
        console.warn("One or more preference form elements not found");
        
        // Even if the form elements aren't present (like on non-preference pages),
        // we may still need to handle the logo based on the current theme
        const currentTheme = document.documentElement.getAttribute("data-theme") || "light";
        updateLogo(currentTheme);
    }
});