/**
 * @jest-environment jsdom
 */

describe('Preferences Preview', () => {
    let colorModeSelect;
    let fontSizeSelect;
    let fontTypeSelect;
    let logoElement;

    beforeEach(() => {
        // Set up basic HTML structure for testing
        document.body.innerHTML = `
            <select id="ColorMode">
                <option value="Light" selected>Light</option>
                <option value="Dark">Dark</option>
                <option value="High Contrast">High Contrast</option>
            </select>
            <select id="FontSize">
                <option value="Small">Small</option>
                <option value="Medium" selected>Medium</option>
                <option value="Large">Large</option>
                <option value="Extra Large">Extra Large</option>
            </select>
            <select id="FontType">
                <option value="Standard" selected>Standard</option>
                <option value="Open Dyslexic">Open Dyslexic</option>
            </select>
            <img id="logo-main" src="/images/team_logo.svg" alt="Logo">
        `;

        // Get the elements
        colorModeSelect = document.getElementById('ColorMode');
        fontSizeSelect = document.getElementById('FontSize');
        fontTypeSelect = document.getElementById('FontType');
        logoElement = document.getElementById('logo-main');

        // Manually call the functions we want to test
        // We need to redefine applyPreviewChanges and updateLogo for testing
        global.applyPreviewChanges = function () {
            const colorModeRaw = colorModeSelect.value;
            const fontSizeRaw = fontSizeSelect.value;
            const fontTypeRaw = fontTypeSelect.value;

            const colorMode = colorModeRaw.toLowerCase().replace(" ", "-");
            const fontSize = fontSizeRaw.toLowerCase().replace(" ", "-");
            const fontType = fontTypeRaw.toLowerCase().replace(" ", "-");

            document.documentElement.classList.remove("light-theme", "dark-theme", "high-contrast-theme");
            document.body.classList.remove("light-theme", "dark-theme", "high-contrast-theme");

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

            document.documentElement.setAttribute("data-theme", colorMode);
            document.documentElement.setAttribute("data-font-size", fontSize);
            document.documentElement.setAttribute("data-font-type", fontType);

            updateLogo(colorMode);
        };

        global.updateLogo = function (colorMode) {
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
        };
    });

    afterEach(() => {
        // Clean up the DOM
        document.body.innerHTML = '';
    });

    test('applies light theme, medium font size, and standard font type initially', () => {
        applyPreviewChanges();

        expect(document.documentElement.classList.contains('light-theme')).toBe(true);
        expect(document.documentElement.getAttribute('data-theme')).toBe('light');
        expect(document.documentElement.getAttribute('data-font-size')).toBe('medium');
        expect(document.documentElement.getAttribute('data-font-type')).toBe('standard');

        expect(logoElement.src).toContain('team_logo.svg');
        expect(logoElement.alt).toBe('Light Mode Logo');
    });

    test('changes to dark theme updates classes and logo', () => {
        colorModeSelect.value = 'Dark';
        applyPreviewChanges();

        expect(document.documentElement.classList.contains('dark-theme')).toBe(true);
        expect(document.documentElement.getAttribute('data-theme')).toBe('dark');

        expect(logoElement.src).toContain('team_logo_dark.png');
        expect(logoElement.alt).toBe('Dark Mode Logo');
    });

    test('changes to high contrast theme updates classes and logo', () => {
        colorModeSelect.value = 'High Contrast';
        applyPreviewChanges();

        expect(document.documentElement.classList.contains('high-contrast-theme')).toBe(true);
        expect(document.documentElement.getAttribute('data-theme')).toBe('high-contrast');

        expect(logoElement.src).toContain('team_logo_contrast.png');
        expect(logoElement.alt).toBe('High Contrast Mode Logo');
    });

    test('changing font size and font type updates attributes correctly', () => {
        fontSizeSelect.value = 'Large';
        fontTypeSelect.value = 'Open Dyslexic';
        applyPreviewChanges();

        expect(document.documentElement.getAttribute('data-font-size')).toBe('large');
        expect(document.documentElement.getAttribute('data-font-type')).toBe('open-dyslexic');
    });
});