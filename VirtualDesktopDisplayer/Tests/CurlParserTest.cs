// Timely API requests for 2025-08-23
// Generated automatically from VirtualDesktop usage data
//
// Instructions:
// 1. Open your browser's Developer Tools (F12)
// 2. Go to the Console tab
// 3. Navigate to: https://app.timelyapp.com/946869/calendar/day?date=2025-08-23
// 4. Copy and paste the code below into the console
// 5. Press Enter to execute all requests

console.log('Starting Timely data entry for 2025-08-23...');

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
            "day": "2025-08-23",
            "note": projectName,
            "timer_state": "default",
            "timer_started_on": 0,
            "timer_stopped_on": 0,
            "project_id": 3572980,
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
                "tl-socket-id": "231821.1020694",
                "x-csrf-token": "Mp-M_Ky6KuNj1T-p7moc408gkrXuW1t17U8xoiWzLlqmEXqwnbToYCpBrHoV4IbqX4BfeP5S0pRzll0HB7jZbg",
                "cookie": "_ga=GA1.1.670192631.1696240092; time_format=24; ajs_user_id=arno.deceuninck@bankvanbreda.be; ajs_anonymous_id=2c101d35-9568-4213-ad75-cf7338a3b770; analytics_session_id=1724425675028; analytics_session_id.last_access=1724425675028; _ga_1JELK6F0SR=GS1.1.1724425674.4.1.1724425678.56.0.0; revision=current; _memory_session=14bf470c2e6c4e93a5826c75812051b6; timely_analytics_session=1755960494124",
                "Referer": "https://app.timelyapp.com/946869/calendar/day?date=2025-08-23&multiUserMode=false"
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

    console.log('ADF Create Schama rechten vraag Simon: 0h 0m');

    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "ADF Create Schama rechten vraag Simon",
        [{"from": "2025-08-23T13:54:15.000+02:00", "to": "2025-08-23T13:54:27.000+02:00", "entry_ids": []}],
        0,211675935
    );

    console.log('github push private repo: 1h 26m');

    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "github push private repo",
        [{"from": "2025-08-23T13:54:27.000+02:00", "to": "2025-08-23T15:20:49.000+02:00", "entry_ids": []}],
        86,36269828333333
    );

    console.log('\n✅ All Timely entries submitted!');
    console.log('Please refresh the page to see your updated timeline.');
}

// Start the submission process
submitAllEntries();
