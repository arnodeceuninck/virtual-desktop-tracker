// Timely API requests for 2025-08-22
// Generated automatically from VirtualDesktop usage data
// 
// Instructions:
// 1. Open your browser's Developer Tools (F12)
// 2. Go to the Console tab
// 3. Navigate to https://app.timelyapp.com/946869/calendar/day?date=2025-08-22
// 4. Copy and paste the code below into the console
// 5. Press Enter to execute all requests

console.log('Starting Timely data entry for 2025-08-22...');

// Helper function to delay between requests
function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// Function to make a Timely API request
async function submitTimelyEntry(projectName, timestamps, totalMinutes) {
    const totalHours = Math.floor(totalMinutes / 60);
    const remainingMinutes = totalMinutes % 60;
    
    const payload = {
        "event": {
            "day": "2025-08-22",
            "note": projectName,
            "timer_state": "default",
            "timer_started_on": 0,
            "timer_stopped_on": 0,
            "project_id": 3572980, // Reusing same project ID for all entries
            "forecast_id": null,
            "label_ids": [],
            "user_ids": [],
            "entry_ids": [],
            "from": timestamps[0].from,
            "to": timestamps[timestamps.length - 1].to,
            "timestamps": timestamps,
            "hours": totalHours,
            "minutes": remainingMinutes,
            "seconds": 0,
            "estimated_hours": 0,
            "estimated_minutes": 0,
            "sequence": 1,
            "billable": false,
            "context": {
                "interaction": "Timestamp Selection",
                "view_context": "Calendar",
                "memory_experience": "Old",
                "memory_view": "Timeline",
                "calendar_view": "Day",
                "has_timer": false
            },
            "state_id": null,
            "billed": false,
            "locked": false,
            "locked_reason": null,
            "external_links": [],
            "user_id": 2190564
        }
    };
    
    try {
        console.log(`Submitting: ${projectName} (${totalHours}h ${remainingMinutes}m)`);
        
        const response = await fetch("https://app.timelyapp.com/946869/hours", {
            "headers": {
                "accept": "application/json",
                "accept-language": "en-US,en;q=0.9,nl;q=0.8",
                "cache-control": "no-cache",
                "content-type": "application/json",
                "pragma": "no-cache",
                "priority": "u=1, i",
                "sec-ch-ua": "\"Not;A=Brand\";v=\"99\", \"Microsoft Edge\";v=\"139\", \"Chromium\";v=\"139\"",
                "sec-ch-ua-mobile": "?0",
                "sec-ch-ua-platform": "\"Windows\"",
                "sec-fetch-dest": "empty",
                "sec-fetch-mode": "same-origin",
                "sec-fetch-site": "same-origin",
                "tl-socket-id": "231680.3423",
                "x-csrf-token": "3EZXy4Hcvlv12FY7uYmzyQ2WC1H44RUbtGWn5kUj5B9IyKGHsNJ82LxMxehCAynAHTbGnOjonPoqvMtDZygTKw",
                "cookie": "_ga=GA1.1.670192631.1696240092; time_format=24; ajs_user_id=arno.deceuninck@bankvanbreda.be; ajs_anonymous_id=2c101d35-9568-4213-ad75-cf7338a3b770; analytics_session_id=1724425675028; analytics_session_id.last_access=1724425675028; _ga_1JELK6F0SR=GS1.1.1724425674.4.1.1724425678.56.0.0; revision=current; _memory_session=14bf470c2e6c4e93a5826c75812051b6; timely_analytics_session=1755792752343",
                "Referer": "https://app.timelyapp.com/946869/calendar/day?date=2025-08-22&multiUserMode=false"
            },
            "body": JSON.stringify(payload),
            "method": "POST"
        });
        
        if (response.ok) {
            console.log(`✅ Successfully submitted: ${projectName}`);
            return await response.json();
        } else {
            console.error(`❌ Failed to submit ${projectName}:`, response.status, response.statusText);
            return null;
        }
    } catch (error) {
        console.error(`❌ Error submitting ${projectName}:`, error);
        return null;
    }
}

// Execute all requests with delays
async function submitAllEntries() {
    console.log('\n=== TIMELY ENTRIES TO SUBMIT ===');

    console.log('Staatsblad Pipeline PRD (incl Docker issues op nieuwe runners): 0h 1m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Staatsblad Pipeline PRD (incl Docker issues op nieuwe runners)",
        [{"from": "2025-08-22T00:07:08.000+02:00", "to": "2025-08-22T00:08:29.000+02:00", "entry_ids": []}],
        1.35
    );
    console.log('blablabla: 8h 49m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "blablabla",
        [{"from": "2025-08-22T00:08:29.000+02:00", "to": "2025-08-22T08:57:38.000+02:00", "entry_ids": []}],
        529.15
    );
    console.log('B1440A runt eeuwig: 0h 11m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "B1440A runt eeuwig",
        [{"from": "2025-08-22T08:57:38.000+02:00", "to": "2025-08-22T09:08:58.000+02:00", "entry_ids": []}],
        11.34
    );
    console.log('ADLS 2 Sharepoint: 0h 23m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "ADLS 2 Sharepoint",
        [{"from": "2025-08-22T09:08:58.000+02:00", "to": "2025-08-22T09:23:06.000+02:00", "entry_ids": []}, {"from": "2025-08-22T14:48:03.000+02:00", "to": "2025-08-22T14:56:56.000+02:00", "entry_ids": []}],
        23.020000000000003
    );
    console.log('Logic Apps CiCD PR: 0h 17m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Logic Apps CiCD PR",
        [{"from": "2025-08-22T09:23:06.000+02:00", "to": "2025-08-22T09:33:35.000+02:00", "entry_ids": []}, {"from": "2025-08-22T09:35:06.000+02:00", "to": "2025-08-22T09:40:52.000+02:00", "entry_ids": []}, {"from": "2025-08-22T12:54:36.000+02:00", "to": "2025-08-22T12:55:26.000+02:00", "entry_ids": []}],
        17.1
    );
    console.log('Sharepoint ADLS: 0h 1m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Sharepoint ADLS",
        [{"from": "2025-08-22T09:33:35.000+02:00", "to": "2025-08-22T09:35:06.000+02:00", "entry_ids": []}],
        1.5
    );
    console.log('AzureML Staatsblad Docker: 1h 50m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "AzureML Staatsblad Docker",
        [{"from": "2025-08-22T09:40:52.000+02:00", "to": "2025-08-22T10:15:58.000+02:00", "entry_ids": []}, {"from": "2025-08-22T10:29:34.000+02:00", "to": "2025-08-22T11:17:46.000+02:00", "entry_ids": []}, {"from": "2025-08-22T12:02:24.000+02:00", "to": "2025-08-22T12:09:11.000+02:00", "entry_ids": []}, {"from": "2025-08-22T12:53:12.000+02:00", "to": "2025-08-22T12:54:36.000+02:00", "entry_ids": []}, {"from": "2025-08-22T13:38:19.000+02:00", "to": "2025-08-22T13:48:17.000+02:00", "entry_ids": []}, {"from": "2025-08-22T13:50:09.000+02:00", "to": "2025-08-22T13:51:59.000+02:00", "entry_ids": []}, {"from": "2025-08-22T14:29:26.000+02:00", "to": "2025-08-22T14:30:42.000+02:00", "entry_ids": []}, {"from": "2025-08-22T14:42:09.000+02:00", "to": "2025-08-22T14:48:03.000+02:00", "entry_ids": []}],
        110.45
    );
    console.log('ADF Create Schama rechten vraag Simon: 0h 13m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "ADF Create Schama rechten vraag Simon",
        [{"from": "2025-08-22T10:15:58.000+02:00", "to": "2025-08-22T10:29:34.000+02:00", "entry_ids": []}],
        13.61
    );
    console.log('Staatsblad Open Tasks Daily: 0h 48m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Staatsblad Open Tasks Daily",
        [{"from": "2025-08-22T11:17:46.000+02:00", "to": "2025-08-22T12:02:24.000+02:00", "entry_ids": []}, {"from": "2025-08-22T12:09:11.000+02:00", "to": "2025-08-22T12:10:41.000+02:00", "entry_ids": []}, {"from": "2025-08-22T13:48:17.000+02:00", "to": "2025-08-22T13:50:09.000+02:00", "entry_ids": []}],
        48.0
    );
    console.log('Screen Off: 0h 42m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Screen Off",
        [{"from": "2025-08-22T12:10:41.000+02:00", "to": "2025-08-22T12:53:12.000+02:00", "entry_ids": []}],
        42.52
    );
    console.log('Desktop 9: 0h 27m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Desktop 9",
        [{"from": "2025-08-22T12:55:26.000+02:00", "to": "2025-08-22T13:23:08.000+02:00", "entry_ids": []}],
        27.7
    );
    console.log('Nieuwe bi repo federated docker credential: 0h 14m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Nieuwe bi repo federated docker credential",
        [{"from": "2025-08-22T13:23:08.000+02:00", "to": "2025-08-22T13:37:25.000+02:00", "entry_ids": []}],
        14.28
    );
    console.log('Compress Logs PR: 0h 0m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Compress Logs PR",
        [{"from": "2025-08-22T13:37:25.000+02:00", "to": "2025-08-22T13:38:19.000+02:00", "entry_ids": []}],
        0.9
    );
    console.log('Desktop 5: 0h 37m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Desktop 5",
        [{"from": "2025-08-22T13:51:59.000+02:00", "to": "2025-08-22T14:29:26.000+02:00", "entry_ids": []}],
        37.45
    );
    console.log('Obscuur problleem simon Simon bytes: 0h 11m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Obscuur problleem simon Simon bytes",
        [{"from": "2025-08-22T14:30:42.000+02:00", "to": "2025-08-22T14:42:09.000+02:00", "entry_ids": []}],
        11.44
    );
    console.log('Selene Schedule If Comare OK: 2h 10m');
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "Selene Schedule If Comare OK",
        [{"from": "2025-08-22T14:56:56.000+02:00", "to": "2025-08-22T17:07:09.000+02:00", "entry_ids": []}],
        130.22
    );

    console.log('\n✅ All Timely entries submitted!');
    console.log('Please refresh the page to see your updated timeline.');
}

// Start the submission process
submitAllEntries();
