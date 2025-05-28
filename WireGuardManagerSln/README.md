# WireGuard Web UI Manager

## Overview

WireGuard Web UI Manager is an ASP.NET Core web application designed to provide a simple and convenient web interface for managing a WireGuard VPN server. It allows administrators to view server status, manage interfaces and peers, and generate client configurations directly from a browser.

## Features

*   **View Server Status:** Check the current status of a WireGuard interface, including its public key and listening port.
*   **Interface Management:** List all available WireGuard interfaces.
*   **Detailed Interface Information:** View detailed configuration and peer information for specific interfaces.
*   **Start/Stop Interfaces:** Easily start and stop WireGuard interfaces using `wg-quick`.
*   **Peer Management:** Add new peers to an interface and remove existing peers.
*   **Key Generation:** Generate public/private keypairs for clients.
*   **Client Configuration Generation:** Generate complete client configuration files (`.conf`).
*   **QR Code Support:** Display client configurations as QR codes for easy import into mobile WireGuard applications.

## Prerequisites

1.  **.NET SDK:** .NET 8.0 SDK or later.
2.  **WireGuard Tools:** The `wg` and `wg-quick` command-line tools must be installed and correctly configured on the server where this application will run. Ensure that WireGuard interface configuration files (e.g., `/etc/wireguard/wg0.conf`) are already set up.
3.  **Permissions:**
    *   The user account running this .NET application needs appropriate permissions to execute `wg` and `wg-quick` commands.
    *   Typically, these commands require `sudo` privileges. You may need to configure `sudoers` to allow the application user to run these specific commands without a password, or ensure the application is run under a user with sufficient privileges. **This is critical for the application to function correctly.**

## Setup & Running the Application

1.  **Clone the Repository (if applicable):**
    ```bash
    git clone <repository_url>
    cd <repository_directory>/WireGuardManagerSln
    ```
    (In the current environment, the code is already present.)

2.  **Navigate to the Web Project:**
    ```bash
    cd WireGuardManager.Web
    ```

3.  **Run the Application:**
    ```bash
    dotnet run
    ```

4.  **Access the Web UI:**
    *   The application will typically be available at `http://localhost:5000` or `https://localhost:5001`. The exact URL will be displayed in the console output when the application starts.

## API Endpoints (Simplified)

The application exposes several API endpoints under `/api/wireguard/`:

*   `GET /status?interfaceName={name}`: Get status of a WireGuard interface.
*   `GET /interfaces`: List all WireGuard interface names.
*   `GET /interfaces/{interfaceName}`: Get detailed data for a specific interface.
*   `POST /interfaces/{interfaceName}/start`: Start an interface.
*   `POST /interfaces/{interfaceName}/stop`: Stop an interface.
*   `GET /generate-keys`: Generate a new public/private keypair.
*   `POST /interfaces/{interfaceName}/peers`: Add a peer to an interface.
*   `DELETE /interfaces/{interfaceName}/peers/{peerPublicKey}`: Remove a peer.
*   `POST /generate-peer-config`: Generate a client configuration file.

## Project Structure

*   **`WireGuardManagerSln/`**: The root directory for the .NET solution.
    *   **`WireGuardManager.Web/`**: The main ASP.NET Core web application.
        *   `Services/WireGuardService.cs`: Contains the core logic for interacting with the WireGuard command-line tools.
        *   `Controllers/WireGuardController.cs`: Defines the API endpoints that the frontend interacts with.
        *   `wwwroot/`: Contains all static frontend assets, including:
            *   `index.html`: The main HTML file for the single-page application.
            *   `css/style.css`: Stylesheets for the application.
            *   `js/site.js`: JavaScript code for frontend logic and API interaction.
        *   `Models/`: Contains C# record definitions for API request bodies.
    *   **`WireGuardManager.Tests/`**: Contains xUnit unit tests for the `WireGuardService`.

## Usage Notes

*   **Direct System Interaction:** This application directly invokes system commands (`wg`, `wg-quick`). Ensure that the environment is secure and that the application is run with appropriate caution.
*   **Permissions are Key:** The most common operational issue will likely be related to permissions. The user running the .NET application *must* have the necessary rights to execute WireGuard commands. If `wg-quick up wg0` fails on the command line for the user, it will also fail for the application.
*   **Existing Interface Configurations:** For interfaces to be started (e.g., `wg0`), a corresponding configuration file (e.g., `/etc/wireguard/wg0.conf`) must exist and be correctly configured on the server. This application manages peers and interface states but does not typically create the initial server-side interface configuration files.

---
This README provides a general guide. Depending on your server setup (OS, existing WireGuard configuration), you might need to adapt paths or commands.The `README.md` file has been successfully created in the `WireGuardManagerSln` directory with the specified content.

**Summary of the README content:**

1.  **Project Title:** "WireGuard Web UI Manager"
2.  **Overview:** Briefly explains that the application is an ASP.NET Core web UI for managing a WireGuard server.
3.  **Features:** Lists key functionalities like viewing status, managing interfaces/peers, generating keys, and client configurations (including QR codes).
4.  **Prerequisites:**
    *   .NET SDK 8.0.
    *   WireGuard tools (`wg`, `wg-quick`) installed and configured.
    *   Crucially, emphasizes the need for appropriate user permissions (e.g., `sudo`) for the application to execute WireGuard commands.
5.  **Setup & Running the Application:**
    *   Provides instructions to navigate to `WireGuardManagerSln/WireGuardManager.Web`.
    *   Specifies `dotnet run` to start the application.
    *   Mentions that the access URL (typically `http://localhost:5000` or `https://localhost:5001`) will be shown in the console.
6.  **API Endpoints:** Includes a simplified list of the main API endpoints for quick reference.
7.  **Project Structure:** Describes the main projects and key files/folders within the solution:
    *   `WireGuardManager.Web` (ASP.NET Core app)
        *   `Services/WireGuardService.cs` (core logic)
        *   `Controllers/WireGuardController.cs` (API endpoints)
        *   `wwwroot/` (frontend assets)
        *   `Models/` (request models)
    *   `WireGuardManager.Tests` (unit tests)
8.  **Usage Notes:**
    *   Highlights direct system command interaction.
    *   Reiterates the critical importance of correct permissions.
    *   Mentions the necessity of pre-existing WireGuard interface configuration files (e.g., `/etc/wireguard/wg0.conf`).

The README is structured to be clear, concise, and provide essential information for users and developers of the application.
All requirements for this subtask have been met.
