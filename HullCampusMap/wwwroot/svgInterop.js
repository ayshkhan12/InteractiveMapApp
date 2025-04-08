window.svgInterop = (function () {
    // Private variables
    const eventHandlers = new WeakMap();
    const highlightClass = 'room-highlight';
    const highlightPulseClass = 'room-highlight-pulse';

    // Private helper functions
    function addRoomEventListeners(element, dotNetRef) {
        const handlers = {
            click: () => {
                dotNetRef.invokeMethodAsync('OnRoomClicked', element.id)
                    .catch(err => console.error('Error invoking OnRoomClicked:', err));
            },
            mouseenter: () => {
                dotNetRef.invokeMethodAsync('OnRoomHover', element.id)
                    .catch(err => console.error('Error invoking OnRoomHover:', err));
            },
            mouseleave: () => {
                dotNetRef.invokeMethodAsync('OnRoomLeave')
                    .catch(err => console.error('Error invoking OnRoomLeave:', err));
            }
        };

        // Store handlers for later cleanup
        eventHandlers.set(element, handlers);

        // Add event listeners
        element.addEventListener('click', handlers.click);
        element.addEventListener('mouseenter', handlers.mouseenter);
        element.addEventListener('mouseleave', handlers.mouseleave);

        // Accessibility improvements
        element.setAttribute('role', 'button');
        element.setAttribute('tabindex', '0');
        element.setAttribute('aria-label', getAccessibleRoomName(element.id));
        element.style.cursor = 'pointer';
    }

    function removeRoomEventListeners(element) {
        const handlers = eventHandlers.get(element);
        if (handlers) {
            element.removeEventListener('click', handlers.click);
            element.removeEventListener('mouseenter', handlers.mouseenter);
            element.removeEventListener('mouseleave', handlers.mouseleave);
            eventHandlers.delete(element);
        }
    }

    function getAccessibleRoomName(roomId) {
        if (roomId.includes('stairs')) return 'Staircase';
        if (roomId.includes('toilet')) return 'Restroom';
        return `Room ${roomId.replace('_', '')}`;
    }

    function isRoomElement(element) {
        const id = element.id;
        return id && (id.startsWith('_') ||
            id.includes('stairs') ||
            id.includes('toilet') ||
            id.includes('elevator'));
    }

    // Public API
    return {
        initialize: function (svgObject, dotNetRef) {
            if (!svgObject) {
                console.error('SVG object is null');
                return;
            }

            const handleSvgLoad = () => {
                try {
                    const svgDoc = svgObject.contentDocument;
                    if (!svgDoc) return;

                    // Process all room elements
                    const roomElements = Array.from(svgDoc.querySelectorAll('path[id], path[data-name]'))
                        .filter(isRoomElement);

                    // Clean up previous listeners
                    roomElements.forEach(el => {
                        removeRoomEventListeners(el);
                        const cleanEl = el.cloneNode(true);
                        el.parentNode.replaceChild(cleanEl, el);
                    });

                    // Add new listeners to clean elements
                    svgDoc.querySelectorAll('path[id], path[data-name]')
                        .filter(isRoomElement)
                        .forEach(el => addRoomEventListeners(el, dotNetRef));

                } catch (error) {
                    console.error('Error initializing SVG interactivity:', error);
                }
            };

            if (svgObject.contentDocument) {
                handleSvgLoad();
            } else {
                svgObject.addEventListener('load', handleSvgLoad);
                // Add error handling for SVG load failures
                svgObject.addEventListener('error', () => {
                    console.error('Failed to load SVG file');
                });
            }
        },

        highlightRoom: function (svgObject, roomId, scrollToView = true) {
            try {
                if (!svgObject || !roomId) return;

                const svgDoc = svgObject.contentDocument;
                if (!svgDoc) return;

                // Clear previous highlights
                svgDoc.querySelectorAll(`.${highlightClass}, .${highlightPulseClass}`).forEach(el => {
                    el.classList.remove(highlightClass, highlightPulseClass);
                });

                const element = svgDoc.getElementById(roomId);
                if (element) {
                    // Add highlight classes
                    element.classList.add(highlightClass);

                    // Add pulse effect for the first 2 seconds
                    element.classList.add(highlightPulseClass);
                    setTimeout(() => {
                        element.classList.remove(highlightPulseClass);
                    }, 2000);

                    if (scrollToView) {
                        element.scrollIntoView({
                            behavior: 'smooth',
                            block: 'center',
                            inline: 'center'
                        });
                    }
                }
            } catch (error) {
                console.error('Error highlighting room:', error);
            }
        },

        cleanup: function (svgObject) {
            if (!svgObject) return;

            try {
                const svgDoc = svgObject.contentDocument;
                if (!svgDoc) return;

                // Remove all event listeners
                svgDoc.querySelectorAll('path[id], path[data-name]')
                    .filter(isRoomElement)
                    .forEach(removeRoomEventListeners);
            } catch (error) {
                console.error('Error during cleanup:', error);
            }
        }
    };
})();