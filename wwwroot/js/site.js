let parkingData = {
    floors: [],
    totalSpots: 0,
    availableSpots: 0,
    occupiedSpots: 0,
    maintenanceSpots: 0,
    buildingId: 1
};

const api_baseUrl = '/api/v1';

// Fetch building data from API
async function fetchBuildingData() {
    try {
        const response = await fetch(`${api_baseUrl}/buildings/${parkingData.buildingId}`);
        if (!response.ok) {
            throw new Error('Failed to fetch building data');
        }

        const buildingData = await response.json();

        // Transform API data to match UI
        parkingData.floors = buildingData.floors.map(floor => ({
            id: floor.id,
            name: floor.name || `Floor ${floor.number}`,
            number: floor.number,
            bays: floor.bays.map(bay => ({
                id: bay.id,
                name: bay.name,
                floorId: bay.floorId,
                buildingId: bay.buildingId,
                spots: bay.spots.map(spot => ({
                    id: spot.id,
                    number: spot.number,
                    name: spot.name,
                    status: spot.status.toLowerCase() || 'available',
                    meta: spot.meta || {},
                    floorId: bay.floorId,
                    bayId: bay.id,
                    buildingId: bay.buildingId,
                    minuteRate: spot.minuteRate || 0.3,
                    car: spot.car || null
                }))
            }))
        }));

        // Calculate total spots
        parkingData.totalSpots = parkingData.floors
            .flatMap(f => f.bays.flatMap(b => b.spots)).length;

        updateStats();

    } catch (error) {
        console.error('Error fetching building data:', error);
    }
}

// API Functions...
async function addFloorApi(floorData) {
    const response = await fetch(`${api_baseUrl}/floors`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...floorData })
    });
    if (!response.ok) throw new Error('Failed to add floor');
    return await response.json();
}

async function deleteFloorApi(floorId) {
    const response = await fetch(`${api_baseUrl}/floors/${floorId}`, {
        method: 'DELETE'
    });
    if (!response.ok) throw new Error('Failed to delete floor');
}

async function addBayApi(bayData) {
    const response = await fetch(`${api_baseUrl}/bays`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(bayData)
    });
    if (!response.ok) throw new Error('Failed to add bay');
    return await response.json();
}

async function deleteBayApi(bayId) {
    const response = await fetch(`${api_baseUrl}/bays/${bayId}`, {
        method: 'DELETE'
    });
    if (!response.ok) throw new Error('Failed to delete bay');
}

async function addSpotApi(spotData) {
    const response = await fetch(`${api_baseUrl}/spots`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(spotData)
    });
    if (!response.ok) throw new Error('Failed to add spot');
    return await response.json();
}

async function deleteSpotApi(spotId) {
    const response = await fetch(`${api_baseUrl}/spots/${spotId}`, {
        method: 'DELETE'
    });
    if (!response.ok) throw new Error('Failed to delete spot');
}

async function updateSpotStatus(spot, newStatus) {
    try {
        // If checking out, check out the car first
        if (newStatus === 'available' && spot.status === 'occupied' && spot.car) {
            const checkoutResponse = await fetch(`${api_baseUrl}/cars/checkout`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    plate: spot.car.plate
                })
            });
            if (!checkoutResponse.ok) throw new Error('Failed to checkout car');
        }

        const response = await fetch(`${api_baseUrl}/spots/status/${spot.id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: newStatus })
        });
        if (!response.ok) throw new Error('Failed to update spot status');

        const modal = bootstrap.Modal.getInstance(document.getElementById('spotModal'));
        modal.hide();

        await fetchBuildingData();
        await fetchAndMatchCarData();
        renderParkingBuilding();
    } catch (error) {
        console.error('Error updating spot status:', error);
        alert('Error: ' + error.message);
    }
}

// Modal Functions
function showAddFloorModal() {
    // Assign next floor number
    document.getElementById('floorNumber').value = parkingData.floors.length + 1;

    const modal = new bootstrap.Modal(document.getElementById('addFloorModal'));
    modal.show();
}

function showAddBayModal(floorId) {
    document.getElementById('bayFloorId').value = floorId;
    const modal = new bootstrap.Modal(document.getElementById('addBayModal'));
    modal.show();
}

function showAddSpotModal(bayId, floorId) {
    document.getElementById('spotBayId').value = bayId;
    document.getElementById('spotFloorId').value = floorId;
    const modal = new bootstrap.Modal(document.getElementById('addSpotModal'));
    modal.show();
}

// Add Functions
async function addFloor() {
    const number = parseInt(document.getElementById('floorNumber').value);
    const buildingId = parkingData.buildingId;

    try {
        const newFloor = await addFloorApi({ number, buildingId });

        // Add to local storage
        parkingData.floors.push({
            id: newFloor.id,
            name: newFloor.name,
            number: newFloor.number,
            bays: []
        });

        bootstrap.Modal.getInstance(document.getElementById('addFloorModal')).hide();
        document.getElementById('floorNumber').value = '';

        renderParkingBuilding();
        updateStats();
    } catch (error) {
        alert('Failed to add floor: ' + error.message);
    }
}

async function addBay() {
    const name = document.getElementById('bayName').value.trim();
    const location = document.getElementById('bayLocation').value.trim();
    const floorId = parseInt(document.getElementById('bayFloorId').value);
    const buildingId = parkingData.buildingId;

    if (!name || !floorId) {
        alert('Please fill in all required fields');
        return;
    }

    try {
        const newBay = await addBayApi({
            name,
            location,
            floorId,
            buildingId
        });

        // Add to local storage
        const floor = parkingData.floors.find(f => f.id === floorId);
        if (floor) {
            floor.bays.push({
                id: newBay.id,
                name: name,
                floorId: floorId,
                buildingId: buildingId,
                spots: []
            });
        }

        bootstrap.Modal.getInstance(document.getElementById('addBayModal')).hide();
        document.getElementById('bayName').value = '';
        document.getElementById('bayLocation').value = '';

        renderParkingBuilding();
        updateStats();
    } catch (error) {
        alert('Failed to add bay: ' + error.message);
    }
}

async function addSpot() {
    const name = document.getElementById('spotName').value.trim();
    const status = document.getElementById('spotStatus').value;
    const bayId = parseInt(document.getElementById('spotBayId').value);
    const floorId = parseInt(document.getElementById('spotFloorId').value);
    const buildingId = parkingData.buildingId;

    if (!name) {
        alert('Please fill in all required fields');
        return;
    }

    try {
        // Find the bay and calculate the next spot number
        const floor = parkingData.floors.find(f => f.id === floorId);
        const bay = floor?.bays.find(b => b.id === bayId);
        const nextNumber = bay?.spots.length > 0
            ? Math.max(...bay.spots.map(s => s.number)) + 1
            : 1;

        const newSpot = await addSpotApi({
            name,
            status,
            number: nextNumber,
            bayId,
            floorId,
            buildingId
        });

        // Update local data
        if (newSpot) {
            bay?.spots.push({
                id: newSpot.id,
                number: nextNumber,
                name: name,
                bayId: bayId,
                floorId: floorId,
                buildingId: buildingId,
                status: status.toLowerCase(),
                car: null
            });
        }

        bootstrap.Modal.getInstance(document.getElementById('addSpotModal')).hide();
        document.getElementById('spotName').value = '';
        document.getElementById('spotStatus').value = 'available';

        renderParkingBuilding();
        updateStats();
    } catch (error) {
        alert('Failed to add spot: ' + error.message);
    }
}

// Delete Functions
async function deleteFloor(floorId) {
    if (!confirm('Are you sure you want to delete this floor and all its bays and spots?')) {
        return;
    }

    try {
        await deleteFloorApi(floorId);

        // Remove from local data
        parkingData.floors = parkingData.floors.filter(f => f.id !== floorId);

        renderParkingBuilding();
        updateStats();
    } catch (error) {
        alert('Failed to delete floor: ' + error.message);
    }
}

async function deleteBay(bayId, floorId) {
    if (!confirm('Are you sure you want to delete this bay and all its spots?')) {
        return;
    }

    try {
        await deleteBayApi(bayId);

        // Remove from local data
        const floor = parkingData.floors.find(f => f.id === floorId);
        if (floor) {
            floor.bays = floor.bays.filter(b => b.id !== bayId);
        }

        renderParkingBuilding();
        updateStats();
    } catch (error) {
        alert('Failed to delete bay: ' + error.message);
    }
}

async function deleteSpot(spotId, bayId, floorId) {
    if (!confirm('Are you sure you want to delete this spot?')) {
        return;
    }

    try {
        await deleteSpotApi(spotId);

        // Remove from local data
        const floor = parkingData.floors.find(f => f.id === floorId);
        const bay = floor?.bays.find(b => b.id === bayId);
        if (bay) {
            bay.spots = bay.spots.filter(s => s.id !== spotId);
        }

        renderParkingBuilding();
        updateStats();
    } catch (error) {
        alert('Failed to delete spot: ' + error.message);
    }
}

function renderParkingBuilding() {
    const container = document.getElementById('floorsContainer');

    container.innerHTML = parkingData.floors.map(floor => `
                <div class="floor">
                    <div class="floor-header">
                        <span>Floor ${floor.number} (${getFloorOccupancy(floor.id)} spots occupied)</span>
                        <div class="floor-controls">
                            <button class="btn btn-success btn-sm" onclick="showAddBayModal(${floor.id})">
                                <i class="fas fa-plus"></i> Add Bay
                            </button>
                            <button class="btn btn-danger btn-sm" onclick="deleteFloor(${floor.id})">
                                <i class="fas fa-trash"></i> Delete Floor
                            </button>
                        </div>
                    </div>
                    <div class="bays-container">
                        ${floor.bays.map(bay => `
                            <div class="bay">
                                <div class="bay-header">
                                    <span>${bay.name}</span>
                                    <div class="bay-controls">
                                       <button class="spot-delete-btn" style="display: initial;" onclick="deleteBay(${bay.id}, ${floor.id})" title="Delete Bay">x</button>
                                    </div>
                                </div>
                                <div class="spots-grid">
                                    ${bay.spots.map(spot => `
                                        <div class="parking-spot ${spot.status}" 
                                             onclick="showSpotDetails('${spot.id}')"
                                             title="Spot ${spot.name} - ${spot.status}">
                                            ${spot.number}
                                            <button class="spot-delete-btn" onclick="event.stopPropagation(); deleteSpot(${spot.id}, ${bay.id}, ${floor.id})" title="Delete Spot">
                                                ×
                                            </button>
                                        </div>
                                    `).join('')}
                                    <div class="add-spot-placeholder" onclick="showAddSpotModal(${bay.id}, ${floor.id})" title="Add New Spot">
                                        +
                                    </div>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
            `).join('');
}

function getFloorOccupancy(floorId) {
    const floor = parkingData.floors.find(f => f.id === floorId);
    return floor.bays.flatMap(b => b.spots).filter(s => s.status === 'occupied' || s.status === 'maintenance').length;
}

function updateStats() {
    const allSpots = parkingData.floors.flatMap(f => f.bays.flatMap(b => b.spots));
    const available = allSpots.filter(s => s.status === 'available').length;
    const occupied = allSpots.filter(s => s.status === 'occupied').length;
    const maintenance = allSpots.filter(s => s.status === 'maintenance').length;

    parkingData.availableSpots = available;
    parkingData.occupiedSpots = occupied;
    parkingData.maintenanceSpots = maintenance;
    parkingData.totalSpots = allSpots.length;

    document.getElementById('totalSpots').textContent = parkingData.totalSpots;
    document.getElementById('availableSpots').textContent = available;
    document.getElementById('occupiedSpots').textContent = occupied;
    document.getElementById('maintenanceSpots').textContent = maintenance;
}

document.addEventListener('click', function(event) {
    const searchContainer = document.querySelector('.search-container');
    const searchResults = document.getElementById('searchResults');
    if (
        searchResults.style.display === 'block' &&
        !searchContainer.contains(event.target)
    ) {
        searchResults.style.display = 'none';
    }
});

function showSpotDetails(spotId) {
    const spot = findSpotById(spotId);
    window.spotId = spot;
    if (!spot) return;

    const modal = new bootstrap.Modal(document.getElementById('spotModal'));
    document.getElementById('spotModalTitle').innerHTML = `Spot ${spot.name} <div class="col"><span class="badge bg-${getStatusColor(spot.status)}">${spot.status.toUpperCase()}</span></div>`;

    const spot_meta = (() => {
        try {
            return JSON.parse(spot.meta);
        } catch {
            return {};
        }
    })();

    let bodyContent = "";
    if (spot.status.toLowerCase() === 'occupied' && spot.car) {
        // Convert check-in to local time
        const checkinLocal = new Date(spot.car.checkin);
        // Get current local time
        const nowLocal = new Date();
        // Calculate duration in minutes
        const duration = Math.floor(Math.abs(nowLocal.getTime() - checkinLocal.getTime()) / (1000 * 60));
        // Calculate the cost
        const cost = duration * spot.minuteRate;

        bodyContent += `
                    <div class="mb-3">
                        <strong>Vehicle:</strong> ${spot.car.plate}
                    </div>
                    <div class="mb-3">
                        <strong>Checked In:</strong> ${spot.car.checkin.toLocaleString()}
                    </div>
                    <div class="mb-3">
                        <strong>Rate:</strong> $${spot.minuteRate.toFixed(2)} per minute ($${cost.toFixed(2)} so far)
                    </div>
                    <div class="mb-3">
                        <strong>Duration:</strong> ${Math.floor(duration / 60)}h ${duration % 60}m
                        <hr>
                    </div>
                `;

    }

    bodyContent += `
                <div class="row mb-3">
                    <div class="col"><strong>Floor:</strong></div>
                    <div class="col">${spot.floorId}</div>
                </div>
                <div class="row mb-3">
                    <div class="col"><strong>Bay:</strong></div>
                    <div class="col">${spot.bayId}</div>
                </div>
                <div class="row mb-3">
                    <div class="col"><strong>Building:</strong></div>
                    <div class="col">${spot.buildingId}</div>
                </div>
            `;

    for (const [key, value] of Object.entries(spot_meta)) {
        bodyContent += `
                    <div class="row mb-3">
                        <div class="col"><strong>${key.replace(/_/g, ' ').replace(/\b\w/g, char => char.toUpperCase())}:</strong></div>
                        <div class="col">${typeof value === 'boolean' ? (value ? 'Yes' : 'No') : (typeof value === 'string' ? value.charAt(0).toUpperCase() + value.slice(1) : value)}</div>
                    </div>
                `;
    }

    document.getElementById('spotModalBody').innerHTML = bodyContent;

    // Update modal footer buttons based on spot status
    const modalFooter = document.querySelector('#spotModal .modal-footer');
    let isOccupied = false;
    let isMaintenance = false;

    isOccupied = spot.status.toLowerCase() === 'occupied' && spot.car;
    isMaintenance = spot.status.toLowerCase() === 'maintenance';

    modalFooter.innerHTML = `<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>`;
    if (isOccupied) {
        modalFooter.innerHTML += `<button type="button" class="btn btn-success" onclick="updateSpotStatus(window.spotId, 'available')">Checkout</button>`;
    } else if (isMaintenance) {
        modalFooter.innerHTML += `<button type="button" class="btn btn-success" onclick="updateSpotStatus(window.spotId, 'available')">Mark as Available</button>`;
        modalFooter.innerHTML += `<button type="button" class="btn btn-danger" onclick="updateSpotStatus(window.spotId, 'occupied')">Mark as Occupied</button>`;
    } else {
        modalFooter.innerHTML += `<button type="button" class="btn btn-success" onclick="showCheckInModal('${spot.id}')">Check In</button>`;
        modalFooter.innerHTML += `<button type="button" class="btn btn-danger" onclick="updateSpotStatus(window.spotId, 'occupied')">Mark as Occupied</button>`;
        modalFooter.innerHTML += `<button type="button" class="btn btn-warning" onclick="updateSpotStatus(window.spotId, 'maintenance')">Mark as Maintenance</button>`;
    }

    modal.show();
}

function findSpotById(spotId) {
    for (const floor of parkingData.floors) {
        for (const bay of floor.bays) {
            const spot = bay.spots.find(s => s.id == spotId);
            if (spot) return spot;
        }
    }
    return null;
}

function getStatusColor(status) {
    switch (status) {
        case 'available': return 'success';
        case 'occupied': return 'danger';
        case 'maintenance': return 'warning';
        default: return 'secondary';
    }
}

function showCheckInModal(spotId) {
    const modal = new bootstrap.Modal(document.getElementById('checkInModal'));
    window.checkInSpotId = spotId;
    loadAvailableCars();
    modal.show();
}

async function loadAvailableCars() {
    try {
        const response = await fetch(`${api_baseUrl}/cars/unparked`);
        if (!response.ok) throw new Error('Failed to fetch available cars');

        const cars = await response.json();
        const availableCars = cars.filter(car => !car.spotId);

        const carsList = document.getElementById('availableCarsList');
        carsList.innerHTML = availableCars.map(car => `
                    <div class="list-group-item list-group-item-action" onclick="checkInCar('${car.plate}')">
                        <strong>${car.plate}</strong>
                    </div>
                `).join('');

        if (availableCars.length === 0) {
            carsList.innerHTML = '<div class="list-group-item text-muted">No available cars to check in</div>';
        }
    } catch (error) {
        console.error('Error loading available cars:', error);
    }
}

async function checkInCar(plate) {
    try {
        try {
            const response = await fetch(`${api_baseUrl}/cars/checkin`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    plate: plate,
                    spotId: window.checkInSpotId
                })
            });

            if (!response.ok) {
                const errorMsg = await response.text();
                throw new Error(`Failed to check in car: ${errorMsg}`);
            }

            bootstrap.Modal.getInstance(document.getElementById('checkInModal')).hide();
            bootstrap.Modal.getInstance(document.getElementById('spotModal')).hide();

            await fetchBuildingData();
            await fetchAndMatchCarData();
            renderParkingBuilding();
        } catch (error) {
            console.error('Error checking in car:', error);
            alert('Error: ' + error.message);
        }

        bootstrap.Modal.getInstance(document.getElementById('checkInModal')).hide();
        bootstrap.Modal.getInstance(document.getElementById('spotModal')).hide();

        await fetchBuildingData();
        await fetchAndMatchCarData();
        renderParkingBuilding();
    } catch (error) {
        console.error('Error checking in car:', error);
        alert('Error: ' + error.message);
    }
}
async function searchCars(query) {
    const resultsContainer = document.getElementById('searchResults');
    resultsContainer.innerHTML = '';
    resultsContainer.style.display = query.trim() ? 'block' : 'none';

    if (!query.trim()) return;

    try {
        const response = await fetch(`${api_baseUrl}/cars?plate=${encodeURIComponent(query)}`);
        if (!response.ok) throw new Error('Failed to fetch cars');

        const cars = await response.json();
        if (cars.length === 0) {
            resultsContainer.innerHTML = '<div class="dropdown-item text-muted">No results found</div>';
            return;
        }

        cars.forEach(car => {
            const item = document.createElement('div');
            item.className = 'dropdown-item';
            item.style.cursor = 'pointer';
            item.innerHTML = `<strong>${car.plate}</strong>`;
            if (car.spotId) {
                const spot = findSpotById(car.spotId);
                if (spot) {
                    const floor = parkingData.floors.find(f => f.id === spot.floorId);
                    const bay = floor?.bays.find(b => b.id === spot.bayId);
                    const spotInfo = `Floor ${floor?.number || spot.floorId}, ${bay?.name || `Bay ${spot.bayId}`}, Spot ${spot.name || spot.number}`;
                    item.innerHTML += ` <span class="badge bg-info">${spotInfo}</span>`;
                } else {
                    item.innerHTML += ` <span class="badge bg-warning">Spot ${car.spotId} (Not Found)</span>`;
                }
            }
            item.onclick = () => selectCar(car);
            resultsContainer.appendChild(item);
        });
    } catch (error) {
        console.error('Error fetching cars:', error);
        resultsContainer.innerHTML = '<div class="dropdown-item text-danger">Error fetching results</div>';
    }
}

async function fetchAndMatchCarData() {
    try {
        const response = await fetch(`${api_baseUrl}/cars`);
        if (!response.ok) throw new Error('Failed to fetch cars');

        const cars = await response.json();

        // Match cars with spots
        cars.forEach(car => {
            if (car.spotId) {
                const spot = findSpotById(car.spotId);
                if (spot) {
                    spot.car = {
                        plate: car.plate,
                        model: car.model,
                        color: car.color,
                        size: car.size,
                        checkin: new Date(car.checkIn)
                    };
                    spot.status = 'occupied';
                }
            }
        });

        updateStats();
    } catch (error) {
        console.error('Error fetching car data:', error);
    }
}

function selectCar(car) {
    document.getElementById('carSearch').value = car.plate;
    document.getElementById('searchResults').style.display = 'none';

    if (car.spotId) {
        const spot = findSpotById(car.spotId);
        if (spot) {
            showSpotDetails(spot.id);
        }
    }
}

// Initialize everything on page load
document.addEventListener('DOMContentLoaded', async function() {
    await fetchBuildingData();
    await fetchAndMatchCarData();
    renderParkingBuilding();
});