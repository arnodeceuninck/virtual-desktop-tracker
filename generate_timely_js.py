#!/usr/bin/env python3
"""
Generate Timely JavaScript files from Virtual Desktop Tracker JSON reports.

This script takes a consolidated JSON report (like usage_report_2025-08-22.json)
and generates a JavaScript file that can be pasted into the browser console
to automatically submit time entries to Timely.

Usage:
    python generate_timely_js.py path/to/usage_report.json
    python generate_timely_js.py path/to/usage_report.json --output custom_filename.js
"""

import json
import os
import argparse
from datetime import datetime
from collections import defaultdict


def merge_consecutive_activities(activities):
    """
    Merge consecutive activities with the same desktop name.
    
    Args:
        activities (list): List of activity records from JSON report
    
    Returns:
        list: List with consecutive identical activities merged
    """
    if not activities:
        return activities
    
    # Sort by start time first
    sorted_activities = sorted(activities, key=lambda x: x['StartTime'])
    merged = []
    
    for activity in sorted_activities:
        if merged and merged[-1]['DesktopName'] == activity['DesktopName']:
            # Extend the previous activity to include this one
            prev_activity = merged[-1]
            prev_activity['EndTime'] = activity['EndTime']
            prev_activity['DurationSeconds'] += activity['DurationSeconds']
            prev_activity['DurationMinutes'] += activity['DurationMinutes']
            prev_activity['DurationFormatted'] = format_duration(prev_activity['DurationSeconds'])
            print(f"Merged consecutive '{activity['DesktopName']}' activities")
        else:
            # Add as new activity
            merged.append(activity.copy())
    
    return merged


def format_duration(seconds):
    """Format duration seconds into human readable format."""
    hours = seconds // 3600
    minutes = (seconds % 3600) // 60
    remaining_seconds = seconds % 60
    
    if hours > 0:
        return f"{hours}h {minutes}m {remaining_seconds}s"
    elif minutes > 0:
        return f"{minutes}m {remaining_seconds}s"
    else:
        return f"{remaining_seconds}s"


def parse_datetime_from_report(datetime_str):
    """Parse datetime string from the JSON report format."""
    # Handle the format used in the JSON report: "2025-08-22 00:07:08"
    return datetime.strptime(datetime_str, '%Y-%m-%d %H:%M:%S')


def generate_timely_javascript(json_report_path, output_path=None):
    """
    Generate a JavaScript file with Timely API requests from a JSON report.
    
    Args:
        json_report_path (str): Path to the JSON report file
        output_path (str, optional): Custom output path for the JS file
    
    Returns:
        str: Path to the generated JavaScript file
    """
    # Load the JSON report
    try:
        with open(json_report_path, 'r', encoding='utf-8') as f:
            report_data = json.load(f)
    except Exception as e:
        print(f"Error loading JSON report: {e}")
        return None
    
    activities = report_data.get('Activities', [])
    if not activities:
        print("No activities found in the JSON report")
        return None
    
    # Extract date from the first activity or filename
    if activities:
        first_activity_date = activities[0]['Date']
        target_date = first_activity_date
    else:
        # Fallback: try to extract date from filename
        filename = os.path.basename(json_report_path)
        if 'usage_report_' in filename:
            date_part = filename.replace('usage_report_', '').replace('.json', '')
            target_date = date_part
        else:
            target_date = datetime.now().strftime('%Y-%m-%d')
    
    print(f"Generating Timely JavaScript for date: {target_date}")
    
    # Group consecutive activities by project name to consolidate timestamps
    project_groups = {}
    
    # First merge consecutive identical activities
    merged_activities = merge_consecutive_activities(activities)
    
    for activity in sorted(merged_activities, key=lambda x: x['StartTime']):
        project_name = activity['DesktopName']
        
        if project_name not in project_groups:
            project_groups[project_name] = []
        
        # Parse the datetime strings from the report
        start_datetime = parse_datetime_from_report(activity['StartTime'])
        end_datetime = parse_datetime_from_report(activity['EndTime'])
        
        project_groups[project_name].append({
            'from': start_datetime.strftime('%Y-%m-%dT%H:%M:%S.000+02:00'),
            'to': end_datetime.strftime('%Y-%m-%dT%H:%M:%S.000+02:00'),
            'duration_minutes': activity['DurationMinutes']
        })
    
    # Generate JavaScript content
    js_content = f"""// Timely API requests for {target_date}
// Generated automatically from VirtualDesktop usage data
// 
// Instructions:
// 1. Open your browser's Developer Tools (F12)
// 2. Go to the Console tab
// 3. Navigate to https://app.timelyapp.com/946869/calendar/day?date={target_date}
// 4. Copy and paste the code below into the console
// 5. Press Enter to execute all requests

console.log('Starting Timely data entry for {target_date}...');

// Helper function to delay between requests
function delay(ms) {{
    return new Promise(resolve => setTimeout(resolve, ms));
}}

// Function to make a Timely API request
async function submitTimelyEntry(projectName, timestamps, totalMinutes) {{
    const totalHours = Math.floor(totalMinutes / 60);
    const remainingMinutes = totalMinutes % 60;
    
    const payload = {{
        "event": {{
            "day": "{target_date}",
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
            "context": {{
                "interaction": "Timestamp Selection",
                "view_context": "Calendar",
                "memory_experience": "Old",
                "memory_view": "Timeline",
                "calendar_view": "Day",
                "has_timer": false
            }},
            "state_id": null,
            "billed": false,
            "locked": false,
            "locked_reason": null,
            "external_links": [],
            "user_id": 2190564
        }}
    }};
    
    try {{
        console.log(`Submitting: ${{projectName}} (${{totalHours}}h ${{remainingMinutes}}m)`);
        
        const response = await fetch("https://app.timelyapp.com/946869/hours", {{
            "headers": {{
                "accept": "application/json",
                "accept-language": "en-US,en;q=0.9,nl;q=0.8",
                "cache-control": "no-cache",
                "content-type": "application/json",
                "pragma": "no-cache",
                "priority": "u=1, i",
                "sec-ch-ua": "\\"Not;A=Brand\\";v=\\"99\\", \\"Microsoft Edge\\";v=\\"139\\", \\"Chromium\\";v=\\"139\\"",
                "sec-ch-ua-mobile": "?0",
                "sec-ch-ua-platform": "\\"Windows\\"",
                "sec-fetch-dest": "empty",
                "sec-fetch-mode": "same-origin",
                "sec-fetch-site": "same-origin",
                "tl-socket-id": "231680.3423",
                "x-csrf-token": "3EZXy4Hcvlv12FY7uYmzyQ2WC1H44RUbtGWn5kUj5B9IyKGHsNJ82LxMxehCAynAHTbGnOjonPoqvMtDZygTKw",
                "cookie": "_ga=GA1.1.670192631.1696240092; time_format=24; ajs_user_id=arno.deceuninck@bankvanbreda.be; ajs_anonymous_id=2c101d35-9568-4213-ad75-cf7338a3b770; analytics_session_id=1724425675028; analytics_session_id.last_access=1724425675028; _ga_1JELK6F0SR=GS1.1.1724425674.4.1.1724425678.56.0.0; revision=current; _memory_session=14bf470c2e6c4e93a5826c75812051b6; timely_analytics_session=1755792752343",
                "Referer": "https://app.timelyapp.com/946869/calendar/day?date={target_date}&multiUserMode=false"
            }},
            "body": JSON.stringify(payload),
            "method": "POST"
        }});
        
        if (response.ok) {{
            console.log(`✅ Successfully submitted: ${{projectName}}`);
            return await response.json();
        }} else {{
            console.error(`❌ Failed to submit ${{projectName}}:`, response.status, response.statusText);
            return null;
        }}
    }} catch (error) {{
        console.error(`❌ Error submitting ${{projectName}}:`, error);
        return null;
    }}
}}

// Execute all requests with delays
async function submitAllEntries() {{
    console.log('\\n=== TIMELY ENTRIES TO SUBMIT ===');
"""

    # Add each project group as a JavaScript function call
    for project_name, timestamps in project_groups.items():
        total_minutes = sum(t['duration_minutes'] for t in timestamps)
        hours = int(total_minutes // 60)
        minutes = int(total_minutes % 60)
        
        js_content += f"\n    console.log('{project_name}: {hours}h {minutes}m');"
        
        # Format timestamps for JavaScript
        timestamp_objects = []
        for ts in timestamps:
            timestamp_objects.append(f'{{"from": "{ts["from"]}", "to": "{ts["to"]}", "entry_ids": []}}')
        
        timestamps_js = "[" + ", ".join(timestamp_objects) + "]"
        
        js_content += f"""
    
    await delay(1000); // Wait 1 second between requests
    await submitTimelyEntry(
        "{project_name}",
        {timestamps_js},
        {total_minutes}
    );"""

    js_content += """

    console.log('\\n✅ All Timely entries submitted!');
    console.log('Please refresh the page to see your updated timeline.');
}

// Start the submission process
submitAllEntries();
"""

    # Determine output filename
    if output_path is None:
        filename = f"timely_entries_{target_date.replace('-', '_')}.js"
        output_path = os.path.join(os.path.dirname(json_report_path), filename)
    
    # Write to file
    try:
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(js_content)
    except Exception as e:
        print(f"Error writing JavaScript file: {e}")
        return None
    
    print(f"\n📄 JavaScript file generated: {os.path.basename(output_path)}")
    print(f"📂 Full path: {output_path}")
    print("\n🔧 Instructions:")
    print("   1. Open your browser's Developer Tools (F12)")
    print("   2. Go to the Console tab")
    print(f"   3. Navigate to: https://app.timelyapp.com/946869/calendar/day?date={target_date}")
    print(f"   4. Copy and paste the content of {os.path.basename(output_path)} into the console")
    print("   5. Press Enter to execute all requests")
    print("\n⚠️  Note: You may need to update the x-csrf-token and cookie values")
    print("   if you get authentication errors. Copy them from a fresh request in Network tab.")
    
    # Print summary
    print(f"\n=== SUMMARY FOR {target_date} ===")
    total_activities = len(project_groups)
    total_time_minutes = sum(sum(t['duration_minutes'] for t in timestamps) for timestamps in project_groups.values())
    total_hours = total_time_minutes / 60
    
    print(f"Total projects: {total_activities}")
    print(f"Total time: {total_hours:.1f} hours ({total_time_minutes:.0f} minutes)")
    
    for project_name, timestamps in sorted(project_groups.items(), 
                                         key=lambda x: sum(t['duration_minutes'] for t in x[1]), 
                                         reverse=True):
        project_minutes = sum(t['duration_minutes'] for t in timestamps)
        project_hours = project_minutes / 60
        percentage = (project_minutes / total_time_minutes) * 100 if total_time_minutes > 0 else 0
        print(f"  {project_name}: {project_hours:.1f}h ({percentage:.1f}%)")
    
    return output_path


def main():
    """Main function with command-line argument parsing."""
    parser = argparse.ArgumentParser(
        description='Generate Timely JavaScript files from Virtual Desktop Tracker JSON reports',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python generate_timely_js.py usage_report_2025-08-22.json
  python generate_timely_js.py path/to/report.json --output my_timely_script.js
  
The generated JavaScript file can be pasted into the browser console on the 
Timely website to automatically submit time entries.
        """
    )
    
    parser.add_argument('json_report', 
                       help='Path to the JSON report file (e.g., usage_report_2025-08-22.json)')
    parser.add_argument('--output', '-o', 
                       help='Custom output filename for the JavaScript file')
    
    args = parser.parse_args()
    
    # Check if input file exists
    if not os.path.exists(args.json_report):
        print(f"❌ Error: Input file does not exist: {args.json_report}")
        return 1
    
    print(f"🔄 Processing JSON report: {args.json_report}")
    
    # Generate the JavaScript file
    result = generate_timely_javascript(args.json_report, args.output)
    
    if result:
        print(f"\n✅ Success! JavaScript file generated successfully.")
        return 0
    else:
        print(f"\n❌ Failed to generate JavaScript file.")
        return 1


if __name__ == "__main__":
    exit(main())
