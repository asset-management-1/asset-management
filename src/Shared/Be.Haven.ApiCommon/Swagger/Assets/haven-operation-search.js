(function () {
    'use strict';

    const nativeFilterInputSelector = '.swagger-ui input[placeholder="Filter by tag"], .swagger-ui .filter input:not(.haven-operation-search-input)';
    const operationSelector = '.swagger-ui .opblock';
    const tagSectionSelector = '.swagger-ui .opblock-tag-section';
    const tagSelector = '.opblock-tag';
    const operationSummarySelector = '.opblock-summary';
    const searchInputClass = 'haven-operation-search-input';
    const searchInputSelector = `.${searchInputClass}`;
    const scopeControlClass = 'haven-operation-search-scope-control';
    const scopeControlSelector = `.${scopeControlClass}`;
    const scopeMenuClass = 'haven-operation-search-scope-menu';
    const scopeMenuSelector = `.${scopeMenuClass}`;
    const scopeOptionClass = 'haven-operation-search-scope-option';
    const searchContainerOpenClass = 'haven-operation-search-container-open';
    const searchContainerClass = 'haven-operation-search-container';
    const hiddenNativeFilterClass = 'haven-swagger-native-filter-hidden';
    const styleElementId = 'haven-operation-search-style';
    const allScopeValue = '__all__';
    const allScopeLabel = 'All controllers';
    const searchPlaceholder = 'Search API by path, method, summary, tag';
    const scopeLabel = 'Search scope';
    let operationIndex = [];
    let indexedSections = [];
    let operationIndexSignature = '';
    let operationIndexReady = false;
    let lastSearchKey = '';
    let pendingSearchFrame = 0;
    let pendingDomSyncFrame = 0;
    let pendingResizeFrame = 0;
    let outsideClickWired = false;
    let resizeListenerWired = false;

    /**
     * Normalizes text for case-insensitive Swagger UI filtering.
     *
     * @param {string} value The raw text to normalize.
     * @returns {string} The trimmed lowercase text used for comparisons.
     */
    function normalize(value) {
        // Use the same lowercase comparison for user input and endpoint text so matching is case-insensitive.
        return (value ?? '').trim().toLowerCase();
    }

    /**
     * Reads the controller or tag name rendered by Swagger UI.
     *
     * @param {Element} section The rendered Swagger tag section.
     * @returns {string} The stable controller/tag name for scope filtering.
     */
    function getTagName(section) {
        // Prefer Swagger's stable tag metadata and fall back to visible text when the metadata is absent.
        const tag = section.querySelector(tagSelector);
        if (!tag) {
            return '';
        }

        const dataTag = tag.dataset.tag;
        if (dataTag) {
            return dataTag;
        }

        const tagLinkText = tag.querySelector('a')?.textContent;
        if (tagLinkText) {
            return tagLinkText.trim();
        }

        const tagClone = tag.cloneNode(true);
        tagClone.querySelectorAll('small, button, svg').forEach((element) => element.remove());
        return (tagClone.textContent ?? '').trim();
    }

    /**
     * Reads the visible tag text rendered by Swagger UI.
     *
     * @param {Element} section The rendered Swagger tag section.
     * @returns {string} The visible tag text used for broad text search.
     */
    function getTagSearchText(section) {
        // Keep the full visible tag text searchable, including the controller description when Swagger renders it.
        const tag = section.querySelector(tagSelector);
        return tag?.textContent ?? '';
    }

    /**
     * Builds searchable text for one rendered Swagger operation.
     *
     * @param {Element} operation The rendered Swagger operation row.
     * @param {string} tagText The visible controller/tag text for the operation.
     * @returns {string} The normalized operation text used by the search filter.
     */
    function getOperationSearchText(operation, tagText) {
        // Combine controller/tag text with the operation row so path, method, and summary all participate.
        const summary = operation.querySelector(operationSummarySelector);
        const operationText = summary?.textContent ?? operation.textContent;
        return normalize(`${tagText} ${operationText}`);
    }

    /**
     * Builds a lightweight signature for the rendered operation list.
     *
     * @param {{section: Element, tagName: string, operations: Element[]}[]} sections The rendered sections to index.
     * @returns {string} The signature used to skip unnecessary index rebuilds.
     */
    function buildOperationIndexSignature(sections) {
        // Track tag names and operation counts; summary text is stable enough to index only when the list changes.
        return sections
            .map((section) => `${section.tagName}:${section.operations.length}`)
            .join('|');
    }

    /**
     * Builds the cached search index from currently rendered Swagger operations.
     *
     * @param {boolean} force Whether to rebuild the index even when the lightweight signature is unchanged.
     * @returns {void}
     */
    function refreshOperationIndex(force = false) {
        // Cache DOM nodes and normalized search text once so typing does not repeatedly query every operation row.
        const sections = Array.from(document.querySelectorAll(tagSectionSelector))
            .map((section) => ({
                section,
                tagName: getTagName(section),
                tagText: getTagSearchText(section),
                operations: Array.from(section.querySelectorAll(operationSelector))
            }));
        const signature = buildOperationIndexSignature(sections);
        if (!force && operationIndexReady && signature === operationIndexSignature) {
            return;
        }

        indexedSections = sections.map((section) => section.section);
        operationIndex = sections.flatMap((section) => {
            const normalizedTag = normalize(section.tagName);

            return section.operations.map((operation) => ({
                section: section.section,
                operation,
                tagName: section.tagName,
                normalizedTag,
                searchText: getOperationSearchText(operation, section.tagText)
            }));
        });
        operationIndexSignature = signature;
        operationIndexReady = true;
        lastSearchKey = '';
    }

    /**
     * Shows or hides an element without removing it from the rendered Swagger DOM.
     *
     * @param {HTMLElement} element The rendered Swagger element to toggle.
     * @param {boolean} isVisible Whether the element should be visible.
     * @returns {void}
     */
    function setVisible(element, isVisible) {
        // Toggle display directly because Swagger UI does not expose a public filtering API for rendered rows.
        const display = isVisible ? '' : 'none';
        if (element.style.display !== display) {
            element.style.display = display;
        }
    }

    /**
     * Finds the Swagger content wrapper that owns the rendered controller sections.
     *
     * @returns {HTMLElement|null} The operation wrapper used as the visual width reference.
     */
    function getOperationWrapper() {
        // Align the custom search bar to Swagger's operation wrapper so it follows the controller group below.
        const section = document.querySelector(tagSectionSelector);
        const wrapper = section?.closest('.wrapper') ?? section?.parentElement;

        return wrapper instanceof HTMLElement ? wrapper : null;
    }

    /**
     * Aligns one search container to the rendered Swagger controller width.
     *
     * @param {Element} container The shared search container.
     * @returns {void}
     */
    function alignSearchContainer(container) {
        // Use the actual rendered controller wrapper width when available and fall back to CSS before render.
        if (!(container instanceof HTMLElement)) {
            return;
        }

        const wrapper = getOperationWrapper();
        if (!wrapper) {
            container.style.removeProperty('width');
            return;
        }

        const wrapperWidth = Math.round(wrapper.getBoundingClientRect().width);
        if (wrapperWidth > 0) {
            container.style.width = `${wrapperWidth}px`;
        }
    }

    /**
     * Aligns every rendered shared Swagger search container.
     *
     * @returns {void}
     */
    function alignSearchContainers() {
        // Keep all custom search bars aligned after Swagger re-renders or the browser viewport changes.
        document
            .querySelectorAll(`.${searchContainerClass}`)
            .forEach((container) => alignSearchContainer(container));
    }

    /**
     * Repositions every open controller scope menu after layout changes.
     *
     * @returns {void}
     */
    function positionOpenScopeMenus() {
        // Keep open menus aligned with their combobox after Swagger layout or viewport width changes.
        document.querySelectorAll(scopeControlSelector).forEach((control) => {
            if (!(control instanceof HTMLButtonElement) || control.getAttribute('aria-expanded') !== 'true') {
                return;
            }

            const menu = getScopeMenu(control);
            if (menu) {
                positionScopeMenu(control, menu);
            }
        });
    }

    /**
     * Wires the resize listener used to keep the search bar aligned with Swagger content.
     *
     * @returns {void}
     */
    function wireSearchResize() {
        // The resize listener is shared across all Swagger documents and throttled to one frame.
        if (resizeListenerWired) {
            return;
        }

        globalThis.addEventListener('resize', () => {
            if (pendingResizeFrame !== 0) {
                globalThis.cancelAnimationFrame(pendingResizeFrame);
            }

            pendingResizeFrame = globalThis.requestAnimationFrame(() => {
                pendingResizeFrame = 0;
                alignSearchContainers();
                positionOpenScopeMenus();
            });
        });
        resizeListenerWired = true;
    }

    /**
     * Resolves the controller scope button paired with the search input.
     *
     * @param {HTMLInputElement} input The Swagger UI search input.
     * @returns {HTMLButtonElement|null} The controller scope button when present.
     */
    function getScopeControl(input) {
        // Resolve the controller scope control from the same filter container as the active search input.
        const container = input.closest(`.${searchContainerClass}`) ?? input.parentElement;
        const control = container?.querySelector(scopeControlSelector) ?? document.querySelector(scopeControlSelector);

        return control instanceof HTMLButtonElement ? control : null;
    }

    /**
     * Reads the currently selected controller scope.
     *
     * @param {HTMLInputElement} input The Swagger UI search input.
     * @returns {string} The selected normalized scope value, or the all-controller value.
     */
    function getSelectedScope(input) {
        // Default to all controllers when the combobox has not been rendered yet.
        const control = getScopeControl(input);
        return control?.dataset.havenScopeValue ?? allScopeValue;
    }

    /**
     * Builds controller scope options from the rendered Swagger tag sections.
     *
     * @returns {{key: string, name: string}[]} The sorted controller scopes available in the current document.
     */
    function getControllerScopes() {
        // Read controller scopes from the cached operation index so this stays cheap during repeated UI sync.
        refreshOperationIndex();

        const scopes = [];
        const seen = new Set();

        operationIndex.forEach((entry) => {
            const key = entry.normalizedTag;
            if (!key || seen.has(key)) {
                return;
            }

            seen.add(key);
            scopes.push({
                key,
                name: entry.tagName
            });
        });

        return scopes.sort((left, right) => left.name.localeCompare(right.name));
    }

    /**
     * Injects the shared Swagger search layout styles once.
     *
     * @returns {void}
     */
    function ensureSearchStyles() {
        // Inject the small layout style once so every service gets the same filter layout.
        if (document.getElementById(styleElementId)) {
            return;
        }

        const style = document.createElement('style');
        style.id = styleElementId;
        style.textContent = `
            .swagger-ui .${searchContainerClass} {
                display: flex !important;
                gap: 0;
                align-items: stretch;
                position: relative;
                --haven-swagger-search-accent: #49cc90;
                --haven-swagger-search-border: rgba(127, 127, 127, 0.7);
                width: min(calc(100% - 96px), 1664px);
                max-width: 100%;
                min-height: 48px;
                margin: 0 auto;
                border-radius: 4px;
            }

            .swagger-ui .${scopeControlClass},
            .swagger-ui .${searchInputClass} {
                box-sizing: border-box;
                min-width: 0;
                width: 100%;
                height: 48px;
                margin: 0 !important;
                padding: 0 12px;
                border: 1px solid var(--haven-swagger-search-border) !important;
                border-radius: 0;
                background-color: transparent !important;
                color: inherit;
                font: inherit;
                line-height: 48px;
                opacity: 0.9;
                vertical-align: top;
            }

            .swagger-ui .${scopeControlClass} {
                cursor: pointer;
                overflow: hidden;
                text-align: left;
                text-overflow: ellipsis;
                white-space: nowrap;
            }

            .swagger-ui .${searchContainerClass}:focus-within .${scopeControlClass},
            .swagger-ui .${searchContainerClass}:focus-within .${searchInputClass},
            .swagger-ui .${searchContainerClass}.${searchContainerOpenClass} .${scopeControlClass},
            .swagger-ui .${searchContainerClass}.${searchContainerOpenClass} .${searchInputClass} {
                border-color: var(--haven-swagger-search-accent) !important;
                opacity: 1;
            }

            .swagger-ui .${scopeControlClass}:hover,
            .swagger-ui .${searchInputClass}:hover {
                border-color: var(--haven-swagger-search-accent) !important;
                opacity: 1;
            }

            .swagger-ui .${scopeControlClass} {
                appearance: none;
                flex: 0 0 clamp(220px, 22vw, 320px);
                padding-right: 48px;
                background-color: transparent !important;
                background-image:
                    linear-gradient(45deg, transparent 50%, currentColor 50%),
                    linear-gradient(135deg, currentColor 50%, transparent 50%);
                background-position:
                    calc(100% - 28px) 50%,
                    calc(100% - 20px) 50%;
                background-repeat: no-repeat;
                background-size: 8px 8px, 8px 8px;
                border-radius: 4px 0 0 4px;
                border-right-width: 0;
            }

            .swagger-ui .${scopeMenuClass} {
                position: absolute;
                top: calc(100% + 4px);
                bottom: auto;
                left: 0;
                z-index: 10000;
                box-sizing: border-box;
                width: clamp(220px, 22vw, 320px);
                max-height: min(320px, 50vh);
                margin: 0;
                padding: 4px;
                overflow-y: auto;
                border: 1px solid var(--haven-swagger-search-border);
                border-radius: 4px;
                background-color: transparent !important;
                color: inherit;
                box-shadow: 0 8px 24px rgba(0, 0, 0, 0.22);
            }

            .swagger-ui .${scopeMenuClass}[hidden] {
                display: none !important;
            }

            .swagger-ui .${scopeOptionClass} {
                display: block;
                width: 100%;
                min-height: 36px;
                padding: 0 12px;
                border: 0;
                border-radius: 3px;
                background: transparent;
                color: inherit;
                font: inherit;
                line-height: 36px;
                text-align: left;
                cursor: pointer;
            }

            .swagger-ui .${scopeOptionClass}:hover,
            .swagger-ui .${scopeOptionClass}:focus,
            .swagger-ui .${scopeOptionClass}[aria-selected="true"] {
                background: rgba(127, 127, 127, 0.24);
                outline: none;
            }

            .swagger-ui .${searchInputClass} {
                flex: 1 1 640px;
                border-radius: 0 4px 4px 0;
            }

            .swagger-ui .${scopeControlClass}:focus,
            .swagger-ui .${scopeControlClass}:focus-visible,
            .swagger-ui .${searchInputClass}:focus,
            .swagger-ui .${searchInputClass}:focus-visible {
                border-color: var(--haven-swagger-search-accent) !important;
                outline: none !important;
                box-shadow: none !important;
            }

            .swagger-ui .${hiddenNativeFilterClass} {
                display: none !important;
            }

            @media (max-width: 900px) {
                .swagger-ui .${searchContainerClass} {
                    width: calc(100% - 32px);
                }

                .swagger-ui .${scopeControlClass} {
                    flex-basis: clamp(160px, 30vw, 240px);
                }

                .swagger-ui .${scopeMenuClass} {
                    width: clamp(160px, 30vw, 240px);
                }
            }

            @media (max-width: 640px) {
                .swagger-ui .${searchContainerClass} {
                    flex-direction: column;
                    align-items: stretch;
                    width: calc(100% - 24px);
                }

                .swagger-ui .${scopeControlClass} {
                    border-radius: 4px 4px 0 0;
                    border-right-width: 1px;
                    border-bottom-width: 0;
                }

                .swagger-ui .${scopeMenuClass} {
                    top: 48px;
                    bottom: auto;
                    width: 100%;
                }

                .swagger-ui .${searchInputClass} {
                    border-radius: 0 0 4px 4px;
                }
            }
        `;

        document.head.appendChild(style);
    }

    /**
     * Builds a compact signature for detecting controller scope changes.
     *
     * @param {{key: string, name: string}[]} scopes The current controller scope list.
     * @returns {string} The scope signature used to skip duplicate option rebuilds.
     */
    function buildScopeSignature(scopes) {
        // Store a compact signature to avoid rebuilding identical controller options on every DOM mutation.
        return scopes.map((scope) => scope.key).join('|');
    }

    /**
     * Builds one option in the custom controller scope menu.
     *
     * @param {string} value The normalized controller scope value.
     * @param {string} label The controller scope label shown to users.
     * @returns {HTMLButtonElement} The custom listbox option button.
     */
    function buildScopeOption(value, label) {
        // Use buttons inside the custom listbox so the dropdown is keyboard reachable and fully themeable.
        const option = document.createElement('button');
        option.type = 'button';
        option.className = scopeOptionClass;
        option.dataset.havenScopeValue = value;
        option.setAttribute('role', 'option');
        option.setAttribute('title', label);
        option.textContent = label;

        return option;
    }

    /**
     * Reads all option buttons inside the custom controller scope menu.
     *
     * @param {HTMLDivElement} menu The custom controller scope menu.
     * @returns {HTMLButtonElement[]} The current rendered scope options.
     */
    function getScopeOptions(menu) {
        // Keep option lookup scoped to the menu so multiple Swagger documents can coexist safely.
        /** @type {HTMLButtonElement[]} */
        const options = [];
        menu.querySelectorAll(`.${scopeOptionClass}`).forEach((option) => {
            if (option instanceof HTMLButtonElement) {
                options.push(option);
            }
        });

        return options;
    }

    /**
     * Moves keyboard focus between controller scope menu options.
     *
     * @param {HTMLDivElement} menu The custom controller scope menu.
     * @param {number} direction The focus direction, where positive moves down and negative moves up.
     * @returns {void}
     */
    function focusScopeOption(menu, direction) {
        // Cycle through rendered options so keyboard navigation stays bounded inside the small menu.
        const options = getScopeOptions(menu);
        if (options.length === 0) {
            return;
        }

        const activeOption = document.activeElement instanceof HTMLButtonElement
            ? document.activeElement
            : null;
        const currentIndex = activeOption ? options.indexOf(activeOption) : -1;
        const nextIndex = currentIndex < 0
            ? 0
            : (currentIndex + direction + options.length) % options.length;
        options[nextIndex].focus();
    }

    /**
     * Finds the custom controller scope menu for the scope control.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @returns {HTMLDivElement|null} The paired custom scope menu.
     */
    function getScopeMenu(control) {
        // Keep the popup lookup scoped to the filter row so multiple Swagger documents do not collide.
        const container = control.closest(`.${searchContainerClass}`) ?? control.parentElement;
        const menu = container?.querySelector(scopeMenuSelector);

        return menu instanceof HTMLDivElement ? menu : null;
    }

    /**
     * Updates the visible scope control label and selected value.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {string} value The normalized scope value.
     * @param {string} label The scope label shown to users.
     * @returns {void}
     */
    function setScopeValue(control, value, label) {
        // Store the normalized key separately from visible text so filtering stays case-insensitive.
        control.dataset.havenScopeValue = value;
        control.textContent = label;
        control.setAttribute('title', label);
    }

    /**
     * Closes the custom controller scope menu.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {HTMLDivElement|null} menu The paired custom scope menu.
     * @returns {void}
     */
    function closeScopeMenu(control, menu) {
        // Collapse the controlled listbox and keep ARIA state in sync for keyboard users.
        control.setAttribute('aria-expanded', 'false');
        const container = control.closest(`.${searchContainerClass}`);
        container?.classList.remove(searchContainerOpenClass);
        if (container instanceof HTMLElement) {
            container.style.removeProperty('margin-bottom');
        }

        if (menu) {
            menu.hidden = true;
        }
    }

    /**
     * Positions the custom controller scope menu without covering the operation list when possible.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {HTMLDivElement} menu The paired custom scope menu.
     * @returns {void}
     */
    function positionScopeMenu(control, menu) {
        // Anchor the custom menu to the real combobox position and reserve space so it does not cover operations.
        menu.style.left = `${control.offsetLeft}px`;
        menu.style.width = `${control.offsetWidth}px`;

        const container = control.closest(`.${searchContainerClass}`);
        if (container instanceof HTMLElement) {
            container.style.marginBottom = `${menu.offsetHeight + 8}px`;
        }
    }

    /**
     * Opens the custom controller scope menu.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {HTMLDivElement|null} menu The paired custom scope menu.
     * @returns {void}
     */
    function openScopeMenu(control, menu) {
        // The menu is custom so it can follow the shared Swagger styling instead of the native browser popup.
        if (!menu) {
            return;
        }

        control.setAttribute('aria-expanded', 'true');
        control.closest(`.${searchContainerClass}`)?.classList.add(searchContainerOpenClass);
        menu.hidden = false;
        positionScopeMenu(control, menu);
    }

    /**
     * Toggles the custom controller scope menu.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @returns {void}
     */
    function toggleScopeMenu(control) {
        // Toggle against the current hidden state instead of relying on focus, which changes while clicking options.
        const menu = getScopeMenu(control);
        if (!menu || menu.hidden) {
            openScopeMenu(control, menu);
            return;
        }

        closeScopeMenu(control, menu);
    }

    /**
     * Handles click selection inside the custom controller scope menu.
     *
     * @param {MouseEvent} event The click event raised by a scope option.
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {HTMLInputElement} input The Haven-owned search input.
     * @returns {void}
     */
    function onScopeMenuClick(event, control, input) {
        // Delegate option clicks from the menu so option rebuilds do not add repeated event handlers.
        if (!(event.target instanceof Element) || !(event.currentTarget instanceof HTMLDivElement)) {
            return;
        }

        const menu = event.currentTarget;
        const option = event.target.closest(`.${scopeOptionClass}`);
        if (!(option instanceof HTMLButtonElement)) {
            return;
        }

        event.preventDefault();
        selectScopeOption(
            control,
            menu,
            input,
            option.dataset.havenScopeValue ?? allScopeValue,
            (option.textContent ?? allScopeLabel).trim());
    }

    /**
     * Handles keyboard navigation inside the custom controller scope menu.
     *
     * @param {KeyboardEvent} event The keyboard event raised by the custom scope menu.
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {HTMLInputElement} input The Haven-owned search input.
     * @returns {void}
     */
    function onScopeMenuKeyDown(event, control, input) {
        // Mirror the small subset of native select behavior needed for fast keyboard use.
        if (!(event.currentTarget instanceof HTMLDivElement)) {
            return;
        }

        const menu = event.currentTarget;

        if (event.key === 'Escape') {
            closeScopeMenu(control, menu);
            control.focus();
            return;
        }

        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
            focusScopeOption(menu, event.key === 'ArrowDown' ? 1 : -1);
            return;
        }

        if (event.key !== 'Enter' && event.key !== ' ') {
            return;
        }

        if (!(event.target instanceof Element)) {
            return;
        }

        const option = event.target.closest(`.${scopeOptionClass}`);
        if (!(option instanceof HTMLButtonElement)) {
            return;
        }

        event.preventDefault();
        selectScopeOption(
            control,
            menu,
            input,
            option.dataset.havenScopeValue ?? allScopeValue,
            (option.textContent ?? allScopeLabel).trim());
    }

    /**
     * Handles keyboard shortcuts on the custom controller scope button.
     *
     * @param {KeyboardEvent} event The keyboard event raised by the scope button.
     * @param {HTMLButtonElement} control The controller scope button.
     * @returns {void}
     */
    function onScopeControlKeyDown(event, control) {
        // Open the custom menu from common combobox keys and move focus to the first available option.
        if (event.key !== 'ArrowDown' && event.key !== 'Enter' && event.key !== ' ') {
            return;
        }

        event.preventDefault();
        const menu = getScopeMenu(control);
        openScopeMenu(control, menu);
        const firstOption = menu?.querySelector(`.${scopeOptionClass}`);
        if (firstOption instanceof HTMLButtonElement) {
            firstOption.focus();
        }
    }

    /**
     * Closes open controller scope menus when users click outside the shared search bar.
     *
     * @param {MouseEvent} event The document click event.
     * @returns {void}
     */
    function closeScopeMenusOnOutsideClick(event) {
        // Custom popups need an outside-click close path because they are not native select elements.
        if (!(event.target instanceof Node)) {
            return;
        }

        document.querySelectorAll(scopeControlSelector).forEach((control) => {
            if (!(control instanceof HTMLButtonElement)) {
                return;
            }

            const container = control.closest(`.${searchContainerClass}`);
            if (container?.contains(event.target)) {
                return;
            }

            closeScopeMenu(control, getScopeMenu(control));
        });
    }

    /**
     * Wires the shared outside-click handler once.
     *
     * @returns {void}
     */
    function wireScopeOutsideClick() {
        // A single document handler handles every custom scope menu across Swagger re-renders.
        if (outsideClickWired) {
            return;
        }

        document.addEventListener('click', closeScopeMenusOnOutsideClick);
        outsideClickWired = true;
    }

    /**
     * Applies one selected controller scope and refreshes the current search.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {HTMLDivElement} menu The paired custom scope menu.
     * @param {HTMLInputElement} input The Haven-owned search input.
     * @param {string} value The selected normalized scope value.
     * @param {string} label The selected scope label.
     * @returns {void}
     */
    function selectScopeOption(control, menu, input, value, label) {
        // Scope selection is a filter change, so update selected state and apply the current text query immediately.
        setScopeValue(control, value, label);
        Array.from(menu.querySelectorAll(`.${scopeOptionClass}`)).forEach((option) => {
            option.setAttribute('aria-selected', option.dataset.havenScopeValue === value ? 'true' : 'false');
        });
        closeScopeMenu(control, menu);
        applySearch(input);
    }

    /**
     * Synchronizes the controller scope menu with the rendered Swagger document.
     *
     * @param {HTMLButtonElement} control The controller scope button.
     * @param {HTMLInputElement} input The Haven-owned search input.
     * @returns {void}
     */
    function syncScopeOptions(control, input) {
        // Rebuild options only when Swagger has rendered a different controller/tag set.
        const scopes = getControllerScopes();
        const signature = buildScopeSignature(scopes);
        if (control.dataset.havenScopeSignature === signature) {
            return;
        }

        const menu = getScopeMenu(control);
        if (!menu) {
            return;
        }

        const currentValue = control.dataset.havenScopeValue ?? allScopeValue;
        menu.replaceChildren();

        const allOption = buildScopeOption(allScopeValue, allScopeLabel);
        menu.appendChild(allOption);

        scopes.forEach((scope) => {
            menu.appendChild(buildScopeOption(scope.key, scope.name));
        });

        const selectedValue = scopes.some((scope) => scope.key === currentValue)
            ? currentValue
            : allScopeValue;
        const selectedLabel = selectedValue === allScopeValue
            ? allScopeLabel
            : scopes.find((scope) => scope.key === selectedValue)?.name ?? allScopeLabel;

        setScopeValue(control, selectedValue, selectedLabel);
        menu.querySelectorAll(`.${scopeOptionClass}`).forEach((option) => {
            option.setAttribute('aria-selected', option.dataset.havenScopeValue === selectedValue ? 'true' : 'false');
        });

        control.dataset.havenScopeSignature = signature;

        if (menu.dataset.havenScopeMenu !== 'true') {
            menu.dataset.havenScopeMenu = 'true';
            menu.addEventListener('click', (event) => onScopeMenuClick(event, control, input));
            menu.addEventListener('keydown', (event) => {
                if (event instanceof KeyboardEvent) {
                    onScopeMenuKeyDown(event, control, input);
                }
            });
        }
    }

    /**
     * Clears Swagger UI's native tag filter so it does not remove operations from the rendered document.
     *
     * @param {HTMLInputElement} nativeInput The Swagger UI built-in filter input.
     * @returns {void}
     */
    function clearNativeFilterInput(nativeInput) {
        // React-backed inputs need the native setter plus an input event to reset Swagger UI's internal filter state.
        if (nativeInput.value.length === 0) {
            return;
        }

        const valueSetter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')?.set;
        if (valueSetter) {
            valueSetter.call(nativeInput, '');
        } else {
            nativeInput.value = '';
        }

        nativeInput.dispatchEvent(new Event('input', { bubbles: true }));
    }

    /**
     * Hides Swagger UI's built-in tag filter after using it as the anchor for shared controls.
     *
     * @param {HTMLInputElement} nativeInput The Swagger UI built-in filter input.
     * @returns {void}
     */
    function hideNativeFilterInput(nativeInput) {
        // The shared search input replaces the native tag-only filter to avoid Swagger mutating the operation list.
        nativeInput.classList.add(hiddenNativeFilterClass);
        nativeInput.setAttribute('aria-hidden', 'true');
        nativeInput.setAttribute('tabindex', '-1');
    }

    /**
     * Creates the Haven-owned search input beside the controller combobox.
     *
     * @param {HTMLInputElement} nativeInput The Swagger UI built-in filter input used as an insertion anchor.
     * @returns {HTMLInputElement|null} The Haven-owned API search input, or null when no container exists.
     */
    function ensureSearchInput(nativeInput) {
        // Keep the custom input in the same filter container so it follows Swagger UI layout and theming.
        const container = nativeInput.closest('.filter') ?? nativeInput.parentElement;
        if (!container) {
            return null;
        }

        ensureSearchStyles();
        container.classList.add(searchContainerClass);
        alignSearchContainer(container);
        wireSearchResize();

        let input = container.querySelector(searchInputSelector);
        if (!(input instanceof HTMLInputElement)) {
            input = document.createElement('input');
            input.type = 'search';
            input.className = searchInputClass;
            input.setAttribute('placeholder', searchPlaceholder);
            input.setAttribute('aria-label', searchPlaceholder);
            input.setAttribute('autocomplete', 'off');
            nativeInput.before(input);
        }

        return input;
    }

    /**
     * Creates and wires the controller scope combobox beside Swagger UI's search input.
     *
     * @param {HTMLInputElement} input The Swagger UI search input.
     * @returns {HTMLButtonElement|null} The wired controller scope combobox, or null when no container exists.
     */
    function wireScopeControl(input) {
        // The controller combobox is attached beside Swagger UI's filter input and reused after re-renders.
        const container = input.closest('.filter') ?? input.parentElement;
        if (!container) {
            return null;
        }

        ensureSearchStyles();
        container.classList.add(searchContainerClass);

        let control = container.querySelector(scopeControlSelector);
        if (!(control instanceof HTMLButtonElement)) {
            control = document.createElement('button');
            control.type = 'button';
            control.className = scopeControlClass;
            control.setAttribute('aria-label', scopeLabel);
            control.setAttribute('aria-haspopup', 'listbox');
            control.setAttribute('aria-expanded', 'false');
            input.before(control);
        }

        let menu = container.querySelector(scopeMenuSelector);
        if (!(menu instanceof HTMLDivElement)) {
            menu = document.createElement('div');
            menu.className = scopeMenuClass;
            menu.setAttribute('role', 'listbox');
            menu.hidden = true;
            control.after(menu);
        }

        if (control.dataset.havenOperationScope !== 'true') {
            control.dataset.havenOperationScope = 'true';
            control.addEventListener('click', (event) => {
                event.preventDefault();
                toggleScopeMenu(control);
            });
            control.addEventListener('keydown', (event) => {
                if (event instanceof KeyboardEvent) {
                    onScopeControlKeyDown(event, control);
                }
            });
        }

        wireScopeOutsideClick();
        syncScopeOptions(control, input);
        return control;
    }

    /**
     * Applies the selected controller scope and text query to the rendered operations.
     *
     * @param {HTMLInputElement} input The Swagger UI search input.
     * @returns {void}
     */
    function applySearch(input) {
        // Filter from the cached DOM index only; this keeps typing responsive on large Swagger documents.
        if (!operationIndexReady) {
            refreshOperationIndex(true);
        }

        const query = normalize(input.value);
        const scope = getSelectedScope(input);
        const searchKey = `${operationIndexSignature}|${scope}|${query}`;
        if (searchKey === lastSearchKey) {
            return;
        }

        const visibleSectionCounts = new Map();
        indexedSections.forEach((section) => visibleSectionCounts.set(section, 0));

        operationIndex.forEach((entry) => {
            const isInScope = scope === allScopeValue || entry.normalizedTag === scope;
            const isVisible = isInScope
                && (query === ''
                    || entry.normalizedTag.includes(query)
                    || entry.searchText.includes(query));

            setVisible(entry.operation, isVisible);

            if (isVisible) {
                visibleSectionCounts.set(entry.section, (visibleSectionCounts.get(entry.section) ?? 0) + 1);
            }
        });

        visibleSectionCounts.forEach((visibleOperationCount, section) => {
            setVisible(section, visibleOperationCount > 0);
        });

        lastSearchKey = searchKey;
    }

    /**
     * Schedules one search pass for the next animation frame.
     *
     * @param {HTMLInputElement} input The Haven-owned search input.
     * @returns {void}
     */
    function scheduleSearch(input) {
        // Collapse rapid keystrokes into one DOM update per frame so typing stays smooth.
        if (pendingSearchFrame !== 0) {
            globalThis.cancelAnimationFrame(pendingSearchFrame);
        }

        pendingSearchFrame = globalThis.requestAnimationFrame(() => {
            pendingSearchFrame = 0;
            applySearch(input);
        });
    }

    /**
     * Schedules one DOM sync pass after Swagger UI re-renders.
     *
     * @returns {void}
     */
    function scheduleDomSync() {
        // Swagger can emit many child-list mutations while rendering; one frame-level sync is enough.
        if (pendingDomSyncFrame !== 0) {
            return;
        }

        pendingDomSyncFrame = globalThis.requestAnimationFrame(() => {
            pendingDomSyncFrame = 0;
            wireSearchInput();
            refreshOperationIndex(true);
            alignSearchContainers();

            const input = document.querySelector(searchInputSelector);
            if (input instanceof HTMLInputElement) {
                wireScopeControl(input);
                applySearch(input);
            }
        });
    }

    /**
     * Handles search input changes and suppresses Swagger UI's default tag-only filter.
     *
     * @param {Event} event The input event raised by Swagger UI's filter box.
     * @returns {void}
     */
    function onSearchInput(event) {
        // The custom input filters the rendered DOM directly, so typing updates results immediately.
        event.stopPropagation();
        event.stopImmediatePropagation();

        const input = event.currentTarget;
        if (!(input instanceof HTMLInputElement)) {
            return;
        }

        scheduleSearch(input);
    }

    /**
     * Applies the current search immediately when Enter is pressed inside the custom input.
     *
     * @param {KeyboardEvent} event The keyboard event raised by the Haven-owned search input.
     * @returns {void}
     */
    function onSearchKeyDown(event) {
        // Prevent form-style submission behavior and keep Enter as an explicit search apply action.
        if (event.key !== 'Enter') {
            return;
        }

        event.preventDefault();

        const input = event.currentTarget;
        if (input instanceof HTMLInputElement) {
            applySearch(input);
        }
    }

    /**
     * Wires Swagger UI's filter input to the shared API search behavior.
     *
     * @returns {void}
     */
    function wireSearchInput() {
        // Swagger UI renders asynchronously, so wire the custom input only after the native filter anchor exists.
        const nativeInput = document.querySelector(nativeFilterInputSelector);
        if (!(nativeInput instanceof HTMLInputElement)) {
            return;
        }

        clearNativeFilterInput(nativeInput);
        hideNativeFilterInput(nativeInput);

        const input = ensureSearchInput(nativeInput);
        if (!input) {
            return;
        }

        if (input.dataset.havenOperationSearch === 'true') {
            return;
        }

        input.dataset.havenOperationSearch = 'true';
        refreshOperationIndex(true);
        wireScopeControl(input);

        input.addEventListener('input', onSearchInput, true);
        input.addEventListener('keyup', onSearchInput, true);
        input.addEventListener('keydown', (event) => {
            if (event instanceof KeyboardEvent) {
                onSearchKeyDown(event);
            }
        }, true);

        applySearch(input);
    }

    /**
     * Observes Swagger UI DOM changes and reapplies the shared search behavior after re-renders.
     *
     * @returns {void}
     */
    function observeSwaggerUi() {
        // Reapply filtering when Swagger UI re-renders after loading a document or changing API versions.
        const observer = new MutationObserver(scheduleDomSync);

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            wireSearchInput();
            observeSwaggerUi();
        });
        return;
    }

    wireSearchInput();
    observeSwaggerUi();
})();

