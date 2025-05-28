document.addEventListener('DOMContentLoaded', () => {
    const API_BASE_URL = '/api/wireguard';

    // --- DOM Elements ---
    const userMessagesDiv = document.getElementById('user-messages');
    // Server Status
    const serverStatusSpan = document.getElementById('server-status');
    const serverPublicKeySpan = document.getElementById('server-public-key');
    const serverListeningPortSpan = document.getElementById('server-listening-port');
    const refreshStatusBtn = document.getElementById('refresh-status-btn');
    // Interfaces
    const interfacesListDiv = document.getElementById('interfaces-list');
    const refreshInterfacesBtn = document.getElementById('refresh-interfaces-btn');
    const addPeerInterfaceSelect = document.getElementById('add-peer-interface-name');
    const addPeerForm = document.getElementById('add-peer-form');
    // Generate Keys
    const generateKeysBtn = document.getElementById('generate-keys-btn');
    const generatedPublicKeyEl = document.getElementById('generated-public-key');
    const generatedPrivateKeyEl = document.getElementById('generated-private-key');
    // Generate Config
    const generateConfigForm = document.getElementById('generate-config-form');
    const generatedPeerConfigTextarea = document.getElementById('generated-peer-config');
    const peerConfigQrCanvas = document.getElementById('peer-config-qr-code');


    // --- Helper Functions ---
    function showMessage(message, isError = false) {
        if (!userMessagesDiv) return;
        userMessagesDiv.textContent = message;
        userMessagesDiv.className = 'user-messages'; // Reset classes
        if (isError) {
            userMessagesDiv.classList.add('error');
        } else {
            userMessagesDiv.classList.add('success');
        }
        // Clear message after some time
        setTimeout(() => {
            userMessagesDiv.textContent = '';
            userMessagesDiv.className = 'user-messages';
        }, 5000);
    }

    async function fetchData(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        try {
            const response = await fetch(url, options);
            if (!response.ok) {
                let errorData;
                try {
                    errorData = await response.json();
                } catch (e) {
                    // Not a JSON error response
                    errorData = { message: `HTTP error! Status: ${response.status}` };
                }
                // Use the 'message' property from the error object if available, otherwise default
                const errorMessage = errorData?.message || `HTTP error! Status: ${response.status}`;
                throw new Error(errorMessage);
            }
            // Handle cases where response might be empty (e.g., 204 No Content for stop/start operations)
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                return await response.json();
            }
            return null; // Or handle as text: await response.text();
        } catch (error) {
            console.error(`Fetch error for ${url}:`, error);
            // error.message should already be set correctly from the block above
            showMessage(error.message, true); // Display the error using showMessage
            throw error; // Re-throw to allow caller to handle if needed
        }
    }

    // --- Server Status Functionality ---
    async function fetchAndDisplayServerStatus() {
        try {
            // Assuming default interface 'wg0' for status for now
            const data = await fetchData('/status?interfaceName=wg0'); 
            if (data) {
                serverStatusSpan.textContent = data.status || 'N/A';
                serverPublicKeySpan.textContent = data.publicKey || 'N/A';
                serverListeningPortSpan.textContent = data.listeningPort || 'N/A';
                
                // Check if the status itself indicates an issue (even if API call was "ok")
                if (data.status && (data.status.toLowerCase().includes("error") || data.status.toLowerCase().includes("not found"))) {
                     // No explicit showMessage here as fetchData would have shown the error from API if response.ok was false
                     // Or if response.ok was true but status is an error, it's an "application level" error from backend
                     // This might be redundant if fetchData already showed a message.
                } else {
                    // Only show success if no prior error message was displayed by fetchData for this operation
                    // This check is tricky. For now, let's assume if data is here, it was a success.
                    // showMessage("Server status refreshed successfully.", false); // Can be noisy
                }
            } else if (serverStatusSpan.textContent === 'Loading...') { 
                // Only set to error if it was initially loading, and no data came back (and no error was thrown by fetchData)
                serverStatusSpan.textContent = 'Status unavailable';
                showMessage('Server status could not be retrieved.', true);
            }
        } catch (error) {
            // Error message is already shown by fetchData's catch block
            serverStatusSpan.textContent = 'Error';
            serverPublicKeySpan.textContent = 'N/A';
            serverListeningPortSpan.textContent = 'N/A';
            // No need for showMessage here, fetchData handles it.
        }
    }

    // --- Interfaces List Functionality ---
    async function fetchAndDisplayInterfaces() {
        try {
            const interfaces = await fetchData('/interfaces');
            interfacesListDiv.innerHTML = ''; // Clear current list
            addPeerInterfaceSelect.innerHTML = ''; // Clear select options

            if (!interfaces || interfaces.length === 0) {
                interfacesListDiv.innerHTML = '<p>No WireGuard interfaces found.</p>';
                const defaultOption = document.createElement('option');
                defaultOption.value = "";
                defaultOption.textContent = "No interfaces available";
                defaultOption.disabled = true;
                addPeerInterfaceSelect.appendChild(defaultOption);
                // showMessage("No WireGuard interfaces found.", false); // Not an error, just info - can be noisy
                return;
            }

            interfaces.forEach(ifName => {
                // Populate interfaces list
                const interfaceItem = document.createElement('div');
                interfaceItem.className = 'interface-item';
                interfaceItem.id = `interface-item-${ifName}`;
                interfaceItem.innerHTML = `
                    <h3>Interface: <span class="interface-name">${ifName}</span></h3>
                    <button class="start-interface-btn" data-interface="${ifName}">Start</button>
                    <button class="stop-interface-btn" data-interface="${ifName}">Stop</button>
                    <button class="view-details-btn" data-interface="${ifName}">View Details/Peers</button>
                    <div class="interface-details" id="interface-details-${ifName}" style="display: none;"></div>
                `;
                interfacesListDiv.appendChild(interfaceItem);

                // Add event listeners for new buttons
                interfaceItem.querySelector('.start-interface-btn').addEventListener('click', () => startInterface(ifName));
                interfaceItem.querySelector('.stop-interface-btn').addEventListener('click', () => stopInterface(ifName));
                interfaceItem.querySelector('.view-details-btn').addEventListener('click', (e) => {
                    const detailsDiv = document.getElementById(`interface-details-${ifName}`);
                    if (detailsDiv.style.display === 'none') {
                        fetchAndDisplayInterfaceDetails(ifName);
                        detailsDiv.style.display = 'block';
                        e.target.textContent = 'Hide Details/Peers';
                    } else {
                        detailsDiv.style.display = 'none';
                        detailsDiv.innerHTML = ''; // Clear details when hiding
                        e.target.textContent = 'View Details/Peers';
                    }
                });

                // Populate select dropdown for "Add Peer" form
                const option = document.createElement('option');
                option.value = ifName;
                option.textContent = ifName;
                addPeerInterfaceSelect.appendChild(option);
            });
            // showMessage("Interfaces list refreshed successfully.", false); // Can be noisy
        } catch (error) {
            interfacesListDiv.innerHTML = '<p>Error loading interfaces.</p>';
            // showMessage already called by fetchData
            const defaultOption = document.createElement('option');
            defaultOption.value = "";
            defaultOption.textContent = "Error loading interfaces";
            defaultOption.disabled = true;
            addPeerInterfaceSelect.appendChild(defaultOption);
        }
    }

    // --- Generate Keys Functionality ---
    async function generateAndDisplayKeys() {
        try {
            const data = await fetchData('/generate-keys'); // Method is GET, no body needed
            if (data && data.publicKey && data.privateKey) {
                generatedPublicKeyEl.textContent = data.publicKey;
                generatedPrivateKeyEl.textContent = data.privateKey;
                showMessage('New keypair generated successfully.', false);
            } else {
                generatedPublicKeyEl.textContent = 'Error';
                generatedPrivateKeyEl.textContent = 'Error';
                // If data is null/undefined but no error was thrown by fetchData, it's an unexpected empty success.
                if (!generatedPublicKeyEl.textContent.includes('Error')) { // Avoid double messaging if fetchData showed an error
                    showMessage('Failed to generate keys (empty or malformed response).', true);
                }
            }
        } catch (error) {
            generatedPublicKeyEl.textContent = 'Error';
            generatedPrivateKeyEl.textContent = 'Error';
            // showMessage already called by fetchData
            // Clear out old interface details sections to prevent stale data
            const allDetailDivs = document.querySelectorAll('.interface-details');
            allDetailDivs.forEach(div => {
                div.innerHTML = '';
                div.style.display = 'none';
            });
            const allViewBtns = document.querySelectorAll('.view-details-btn');
            allViewBtns.forEach(btn => btn.textContent = 'View Details/Peers');

        }
    }

    // --- Interface Actions (Start/Stop) ---
    async function startInterface(interfaceName) {
        try {
            await fetchData(`/interfaces/${interfaceName}/start`, { method: 'POST' });
            showMessage(`Interface ${interfaceName} started successfully.`, false);
            fetchAndDisplayServerStatus(); // Refresh status
        } catch (error) {
            // showMessage already handled by fetchData
            // showMessage(`Error starting interface ${interfaceName}: ${error.message}`, true);
        }
    }

    async function stopInterface(interfaceName) {
        try {
            await fetchData(`/interfaces/${interfaceName}/stop`, { method: 'POST' });
            showMessage(`Interface ${interfaceName} stopped successfully.`, false);
            fetchAndDisplayServerStatus(); // Refresh status
        } catch (error) {
            // showMessage(`Error stopping interface ${interfaceName}: ${error.message}`, true);
        }
    }

    // --- View Interface Details / Peers ---
    async function fetchAndDisplayInterfaceDetails(interfaceName) {
        const detailsDiv = document.getElementById(`interface-details-${interfaceName}`);
        if (!detailsDiv) return;

        detailsDiv.innerHTML = '<p>Loading details...</p>'; // Show loading indicator

        try {
            const data = await fetchData(`/interfaces/${interfaceName}`);
            if (!data || !data.details) { // Assuming controller returns { details: "raw string" }
                detailsDiv.innerHTML = '<p>No details available for this interface.</p>';
                return;
            }

            const rawDetails = data.details;
            detailsDiv.innerHTML = ''; // Clear loading indicator

            // Basic parsing of wg show output
            const lines = rawDetails.split('\n');
            let currentPeerPublicKey = null;
            const peersMap = new Map(); // Store peers to group allowed IPs under correct peer

            lines.forEach(line => {
                const trimmedLine = line.trim();
                if (trimmedLine.startsWith('peer:')) {
                    currentPeerPublicKey = trimmedLine.substring('peer:'.length).trim();
                    if (!peersMap.has(currentPeerPublicKey)) {
                        peersMap.set(currentPeerPublicKey, { publicKey: currentPeerPublicKey, allowedIPs: 'N/A' });
                    }
                } else if (trimmedLine.startsWith('allowed ips:') && currentPeerPublicKey) {
                    if (peersMap.has(currentPeerPublicKey)) {
                        peersMap.get(currentPeerPublicKey).allowedIPs = trimmedLine.substring('allowed ips:'.length).trim();
                    }
                }
            });
            
            const peersList = document.createElement('ul');
            peersList.className = 'peers-list';

            if (peersMap.size > 0) {
                peersMap.forEach(peer => {
                    const peerItem = document.createElement('li');
                    peerItem.className = 'peer-item';
                    peerItem.innerHTML = `
                        Peer Public Key: <span class="peer-public-key">${peer.publicKey}</span><br>
                        Allowed IPs: <span class="peer-allowed-ips">${peer.allowedIPs}</span><br>
                        <button class="remove-peer-btn" data-interface="${interfaceName}" data-peerkey="${peer.publicKey}">Remove Peer</button>
                    `;
                    peerItem.querySelector('.remove-peer-btn').addEventListener('click', () => removePeer(interfaceName, peer.publicKey));
                    peersList.appendChild(peerItem);
                });
            } else {
                peersList.innerHTML = '<li>No peers configured for this interface.</li>';
            }
            
            const peersHeader = document.createElement('h4');
            peersHeader.textContent = 'Peers:';
            detailsDiv.appendChild(peersHeader);
            detailsDiv.appendChild(peersList);

            const rawDataHeader = document.createElement('h4');
            rawDataHeader.textContent = 'Raw Configuration Data:';
            detailsDiv.appendChild(rawDataHeader);
            const rawDataPre = document.createElement('pre');
            rawDataPre.className = 'interface-raw-data';
            rawDataPre.textContent = rawDetails;
            detailsDiv.appendChild(rawDataPre);

        } catch (error) {
            detailsDiv.innerHTML = `<p>Error loading interface details: ${error.message}</p>`;
            // showMessage already handled by fetchData
        }
    }
    
    // --- Remove Peer ---
    async function removePeer(interfaceName, peerPublicKey) {
        if (!confirm(`Are you sure you want to remove peer ${peerPublicKey} from ${interfaceName}?`)) {
            return;
        }
        try {
            // Peer public keys can contain '+', which needs to be encoded if part of URL path segment
            await fetchData(`/interfaces/${interfaceName}/peers/${encodeURIComponent(peerPublicKey)}`, { method: 'DELETE' });
            showMessage(`Peer ${peerPublicKey} removed successfully from ${interfaceName}.`, false);
            fetchAndDisplayInterfaceDetails(interfaceName); // Refresh details
        } catch (error) {
            // showMessage(`Error removing peer: ${error.message}`, true);
        }
    }


    // --- Event Listeners ---
    if (refreshStatusBtn) {
        refreshStatusBtn.addEventListener('click', fetchAndDisplayServerStatus);
    }
    if (refreshInterfacesBtn) {
        refreshInterfacesBtn.addEventListener('click', fetchAndDisplayInterfaces);
    }
    if (generateKeysBtn) {
        generateKeysBtn.addEventListener('click', generateAndDisplayKeys);
    }
    if (addPeerForm) {
        addPeerForm.addEventListener('submit', handleAddPeerSubmit);
    }
    if (generateConfigForm) {
        generateConfigForm.addEventListener('submit', handleGenerateConfigSubmit);
    }

    // --- Add Peer Form Submission ---
    async function handleAddPeerSubmit(event) {
        event.preventDefault();
        const interfaceName = addPeerForm.elements.interfaceName.value;
        const publicKey = addPeerForm.elements.publicKey.value;
        const allowedIPs = addPeerForm.elements.allowedIPs.value;

        if (!interfaceName || !publicKey || !allowedIPs) {
            showMessage('All fields are required for adding a peer.', true);
            return;
        }

        try {
            await fetchData(`/interfaces/${interfaceName}/peers`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ publicKey, allowedIPs })
            });
            showMessage(`Peer added successfully to ${interfaceName}.`, false);
            fetchAndDisplayInterfaceDetails(interfaceName); // Refresh details for the affected interface
            addPeerForm.reset(); // Clear form
        } catch (error) {
            // showMessage(`Error adding peer: ${error.message}`, true);
        }
    }

    // --- Generate Peer Configuration Form Submission ---
    async function handleGenerateConfigSubmit(event) {
        event.preventDefault();
        const form = event.target;
        const peerPrivateKey = form.elements.peerPrivateKey.value;
        const peerPresharedKey = form.elements.peerPresharedKey.value; // Optional
        const peerDnsServersRaw = form.elements.peerDnsServers.value; // Optional, comma-separated
        const interfaceAddress = form.elements.interfaceAddress.value;
        const serverPublicKey = form.elements.serverPublicKey.value;
        const serverEndpoint = form.elements.serverEndpoint.value;
        const serverAllowedIPs = form.elements.serverAllowedIPs.value;

        if (!peerPrivateKey || !interfaceAddress || !serverPublicKey || !serverEndpoint || !serverAllowedIPs) {
            showMessage('Please fill in all required fields for configuration generation.', true);
            return;
        }

        const requestBody = {
            peerPrivateKey,
            peerPresharedKey: peerPresharedKey || null,
            peerDnsServers: peerDnsServersRaw ? peerDnsServersRaw.split(',').map(s => s.trim()).filter(s => s) : [],
            interfaceAddress,
            serverPublicKey,
            serverEndpoint,
            serverAllowedIPs
        };

        try {
            const result = await fetchData('/generate-peer-config', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(requestBody)
            });

            if (result && result.peerConfig) {
                generatedPeerConfigTextarea.value = result.peerConfig;
                showMessage('Peer configuration generated successfully.', false);

                // Generate QR Code
                if (peerConfigQrCanvas && typeof QRious !== 'undefined') {
                    try {
                        new QRious({
                            element: peerConfigQrCanvas,
                            value: result.peerConfig,
                            size: 200, // Adjust size as needed
                            level: 'L' // Error correction level (L, M, Q, H)
                        });
                         peerConfigQrCanvas.style.display = 'block'; // Show canvas
                    } catch (qrError) {
                        console.error("QR Code generation failed:", qrError);
                        showMessage("Configuration generated, but QR code display failed.", true);
                        peerConfigQrCanvas.style.display = 'none'; // Hide canvas on error
                    }
                } else if (typeof QRious === 'undefined') {
                     console.warn("QRious library not found. Cannot generate QR code.");
                     showMessage("Configuration generated, but QRious library not found for QR code.", true);
                }


            } else {
                generatedPeerConfigTextarea.value = '';
                if (peerConfigQrCanvas) peerConfigQrCanvas.style.display = 'none';
                // Error message might have been shown by fetchData if it was an API error
                // If it's a valid response but no peerConfig, then show specific error.
                if (!generatedPeerConfigTextarea.value.includes('Error')) {
                     showMessage('Failed to generate peer configuration (empty or malformed response).', true);
                }
            }
        } catch (error) {
            generatedPeerConfigTextarea.value = 'Error generating configuration.';
            if (peerConfigQrCanvas) peerConfigQrCanvas.style.display = 'none';
            // showMessage(`Error generating peer configuration: ${error.message}`, true);
        }
    }


    // --- Initial Load ---
    fetchAndDisplayServerStatus();
    fetchAndDisplayInterfaces();
});
