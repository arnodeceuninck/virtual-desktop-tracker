import json
import os
import glob
from datetime import datetime, timedelta
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
from matplotlib.patches import Rectangle
import numpy as np
from collections import defaultdict
import colorsys
import argparse
import math

folder = r"C:\Users\ANK\OneDrive - Bank Van Breda\Documents\VirtualDesktopLogs"

def ceil_to_minute(minutes):
    """Ceil minutes to the nearest minute."""
    return math.ceil(minutes)

def consolidate_short_activities(records, min_duration_minutes=2):
    """
    Consolidate activities based on size - smallest activities get consolidated first.
    Duration gets added to the larger of the adjacent activities.
    All durations are ceiled to the nearest minute and no overlapping records exist.
    
    Args:
        records (list): List of parsed desktop usage records
        min_duration_minutes (float): Minimum duration in minutes for an activity to be kept separate
    
    Returns:
        list: Consolidated list of records
    """
    if not records:
        return records
    
    # Now consolidate smallest activities first
    while True:
        # Find activities smaller than minimum duration
        small_activities = [(i, record) for i, record in enumerate(records) 
                          if record['duration_minutes'] < min_duration_minutes]
        
        if not small_activities:
            break  # No more small activities to consolidate
        
        # Find the smallest activity
        smallest_idx, smallest_record = min(small_activities, key=lambda x: x[1]['duration_minutes'])
        
        # Find adjacent activities
        prev_idx = smallest_idx - 1 if smallest_idx > 0 else None
        next_idx = smallest_idx + 1 if smallest_idx < len(records) - 1 else None
        
        # Determine which adjacent activity to merge with (the larger one)
        merge_with_idx = None
        if prev_idx is not None and next_idx is not None:
            # Both adjacent activities exist, choose the larger one
            prev_duration = records[prev_idx]['duration_minutes']
            next_duration = records[next_idx]['duration_minutes']
            merge_with_idx = prev_idx if prev_duration >= next_duration else next_idx
        elif prev_idx is not None:
            merge_with_idx = prev_idx
        elif next_idx is not None:
            merge_with_idx = next_idx
        else:
            # No adjacent activities, keep this one
            break
        
        # Merge the smallest activity with the chosen adjacent activity
        target_record = records[merge_with_idx]
        
        print(f"Consolidating '{smallest_record['desktop']}' ({smallest_record['duration_minutes']:.0f}min) "
              f"into '{target_record['desktop']}' ({target_record['duration_minutes']:.0f}min)")
        
        if merge_with_idx < smallest_idx:
            # Merging with previous activity - extend its end time
            target_record['duration_minutes'] += smallest_record['duration_minutes']
            target_record['end_minutes'] = smallest_record['end_minutes']
            target_record['end_time'] = smallest_record['end_time']
        else:
            # Merging with next activity - extend its start time backward
            target_record['duration_minutes'] += smallest_record['duration_minutes']
            target_record['start_minutes'] = smallest_record['start_minutes']
            target_record['start_time'] = smallest_record['start_time']
        
        # Remove the smallest activity
        records.pop(smallest_idx)
        
        print(f"Result: '{target_record['desktop']}' now {target_record['duration_minutes']:.0f}min")

    # Final pass to floor all start and end times
    for record in records:
        # Update start_time and end_time
        record['start_time'] = record['start_time'].replace(second=0, microsecond=0)
        record['end_time'] = record['end_time'].replace(second=0, microsecond=0)
        # Update duration_minutes
        record['duration_minutes'] = (record['end_minutes'] - record['start_minutes'])

    return records

def custom_record_processing(records):
    """
    Some custom, use case specific handling
    - If desktop name is "Desktop n", where n is a number, and the time is less than 15 min, consolidate
       with the next record (same for "General")
    - If desktop name is "Screen Off" (and the duration is less than 15 min) consolidate with the record before it
    """
    # Keep processing until no more consolidations are made
    changes_made = True
    while changes_made:
        changes_made = False
        
        for i in range(len(records) - 1, -1, -1):  # Iterate backwards
            current = records[i]
            
            if (current['desktop'].startswith("Desktop ") or current['desktop'] == "General") and current['duration_minutes'] < 15:
                if i + 1 < len(records):
                    next_record = records[i + 1]
                    print(f"Consolidating '{current['desktop']}' into '{next_record['desktop']}'")
                    next_record['start_minutes'] = current['start_minutes']
                    next_record['start_time'] = current['start_time']
                    next_record['duration_minutes'] += current['duration_minutes']
                    records.pop(i)
                    changes_made = True
                    break  # Start over from the end after making a change
                    
            elif current['desktop'] == "Screen Off" and current['duration_minutes'] < 15:
                if i - 1 >= 0:
                    prev_record = records[i - 1]
                    print(f"Consolidating '{current['desktop']}' into '{prev_record['desktop']}'")
                    prev_record['end_minutes'] = current['end_minutes']
                    prev_record['end_time'] = current['end_time']
                    prev_record['duration_minutes'] += current['duration_minutes']
                    records.pop(i)
                    changes_made = True
                    break  # Start over from the end after making a change

    return records

def merge_consecutive_activities(records):
    """
    Merge consecutive activities with the same desktop name.
    
    Args:
        records (list): List of desktop usage records
    
    Returns:
        list: List with consecutive identical activities merged
    """
    if not records:
        return records
    
    # Sort by start time first
    sorted_records = sorted(records, key=lambda x: x['start_minutes'])
    merged = []
    
    for record in sorted_records:
        if merged and merged[-1]['desktop'] == record['desktop']:
            # Extend the previous record to include this one
            prev_record = merged[-1]
            prev_record['end_time'] = record['end_time']
            prev_record['end_minutes'] = record['end_minutes']
            prev_record['duration_minutes'] = prev_record['end_minutes'] - prev_record['start_minutes']
            print(f"Merged consecutive '{record['desktop']}' activities")
        else:
            # Add as new record
            merged.append(record.copy())
    
    return merged

def print_daily_timeline(records):
    """
    Print a formatted timeline of the entire day's activities.
    
    Args:
        records (list): List of consolidated desktop usage records
    """
    if not records:
        print("No activities to display")
        return
    
    # First merge consecutive identical activities
    merged_records = merge_consecutive_activities(records)
    
    print("\n" + "="*50)
    print("DAILY ACTIVITY TIMELINE")
    print("="*50)
    
    for record in sorted(merged_records, key=lambda x: x['start_minutes']):
        # Convert minutes to proper time format
        start_total_mins = record['start_minutes']
        end_total_mins = record['end_minutes']
        
        start_hours = int(start_total_mins // 60)
        start_mins = int(start_total_mins % 60)
        end_hours = int(end_total_mins // 60)
        end_mins = int(end_total_mins % 60)
        
        # Handle times that go past 24 hours (next day)
        if start_hours >= 24:
            start_hours = start_hours % 24
        if end_hours >= 24:
            end_hours = end_hours % 24
        
        start_str = f"{start_hours:02d}:{start_mins:02d}"
        end_str = f"{end_hours:02d}:{end_mins:02d}"
        
        print(f"{start_str}-{end_str} {record['desktop']}")
    
    print("="*50)

def generate_timely_javascript(records, target_date):
    """
    Generate a JavaScript file with Timely API requests for the consolidated timeline.
    
    Args:
        records (list): List of consolidated desktop usage records
        target_date (str): Date in format 'YYYY-MM-DD'
    """
    if not records:
        print("No records to generate Timely JavaScript")
        return
    
    # Group consecutive activities by project name to consolidate timestamps
    project_groups = {}
    
    # First merge consecutive identical activities
    merged_records = merge_consecutive_activities(records)
    
    for record in sorted(merged_records, key=lambda x: x['start_minutes']):
        project_name = record['desktop']
        
        if project_name not in project_groups:
            project_groups[project_name] = []
        
        # Convert minutes back to datetime for proper formatting
        start_datetime = record['start_time']
        end_datetime = record['end_time']
        
        project_groups[project_name].append({
            'from': start_datetime.strftime('%Y-%m-%dT%H:%M:%S.000+02:00'),
            'to': end_datetime.strftime('%Y-%m-%dT%H:%M:%S.000+02:00'),
            'duration_minutes': record['duration_minutes']
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

    # Write to file
    filename = f"timely_entries_{target_date.replace('-', '_')}.js"
    filepath = os.path.join(os.path.dirname(__file__), filename)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(js_content)
    
    print(f"\n📄 JavaScript file generated: {filename}")
    print("🔧 Instructions:")
    print("   1. Open your browser's Developer Tools (F12)")
    print("   2. Go to the Console tab")
    print(f"   3. Navigate to: https://app.timelyapp.com/946869/calendar/day?date={target_date}")
    print(f"   4. Copy and paste the content of {filename} into the console")
    print("   5. Press Enter to execute all requests")
    print("\n⚠️  Note: You may need to update the x-csrf-token and cookie values")
    print("   if you get authentication errors. Copy them from a fresh request in Network tab.")

def load_daily_logs(target_date=None):
    """
    Load and combine all VirtualDesktop logs for a specific date.
    
    Args:
        target_date (str, optional): Date in format 'YYYY-MM-DD'. If None, uses today.
    
    Returns:
        list: Combined and sorted list of desktop usage records
    """
    if target_date is None:
        target_date = datetime.now().strftime('%Y-%m-%d')
    
    # Find all log files for the target date
    pattern = os.path.join(folder, f"VirtualDesktopUsage_{target_date}_*.json")
    log_files = glob.glob(pattern)
    
    if not log_files:
        print(f"No log files found for date {target_date}")
        return []
    
    all_records = []
    
    for file_path in log_files:
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                records = json.load(f)
                all_records.extend(records)
                print(f"Loaded {len(records)} records from {os.path.basename(file_path)}")
        except Exception as e:
            print(f"Error loading {file_path}: {e}")
    
    # Sort by start time
    all_records.sort(key=lambda x: x['StartTime'])
    
    print(f"Total records loaded: {len(all_records)}")
    return all_records

def generate_colors(n):
    """Generate n distinct colors for desktop activities."""
    colors = []
    for i in range(n):
        hue = i / n
        saturation = 0.7 + (i % 3) * 0.1  # Vary saturation slightly
        lightness = 0.6 + (i % 2) * 0.2   # Vary lightness slightly
        rgb = colorsys.hls_to_rgb(hue, lightness, saturation)
        colors.append(rgb)
    return colors

def visualize_desktop_usage(target_date=None, figsize=(16, 10), detail=False):
    """
    Create a timeline visualization of desktop usage with 15-minute intervals.
    
    Args:
        target_date (str, optional): Date in format 'YYYY-MM-DD'. If None, uses today.
        figsize (tuple): Figure size for the plot
        detail (bool): If True, show all activities. If False, consolidate activities < 5 minutes.
    """
    records = load_daily_logs(target_date)
    
    if not records:
        print("No data to visualize")
        return
    
    if target_date is None:
        target_date = datetime.now().strftime('%Y-%m-%d')
    
    # Sort records by start time first to identify the actual last record
    records.sort(key=lambda x: x['StartTime'])
    
    # Find the chronologically last record with null EndTime
    last_record_idx = None
    for i in range(len(records) - 1, -1, -1):
        if not records[i]['EndTime']:
            last_record_idx = i
            break
    
    # Parse datetime strings and calculate durations
    parsed_records = []
    for i, record in enumerate(records):
        start_time = datetime.fromisoformat(record['StartTime'].replace('Z', '+00:00'))
        
        if record['EndTime']:
            end_time = datetime.fromisoformat(record['EndTime'].replace('Z', '+00:00'))
        elif i == last_record_idx:
            # Only set current time for the actual last record with null EndTime
            end_time = datetime.now(start_time.tzinfo)
            print(f"Setting current time as end time for last record: {record['DesktopName']}")
        else:
            # For other records with null EndTime, skip them or use start time + 1 minute as default
            print(f"Skipping record with null EndTime (not last): {record['DesktopName']} at {start_time.strftime('%H:%M:%S')}")
            continue
        
        # Convert to minutes from start of day
        start_minutes = start_time.hour * 60 + start_time.minute + start_time.second / 60
        end_minutes = end_time.hour * 60 + end_time.minute + end_time.second / 60
        
        # Handle day rollover
        if end_minutes < start_minutes:
            end_minutes += 24 * 60  # Add 24 hours worth of minutes
        
        parsed_records.append({
            'desktop': record['DesktopName'],
            'start_time': start_time,
            'end_time': end_time,
            'start_minutes': start_minutes,
            'end_minutes': end_minutes,
            'duration_minutes': end_minutes - start_minutes
        })
    
    # Consolidate short activities if not in detail mode
    if not detail:
        print(f"\nConsolidating activities shorter than 5 minutes...")
        original_count = len(parsed_records)
        parsed_records = consolidate_short_activities(parsed_records)
        consolidated_count = len(parsed_records)
        print(f"Reduced from {original_count} to {consolidated_count} activities")
    else:
        # Even in detail mode, ceil durations to minutes and fix overlaps
        for record in parsed_records:
            record['duration_minutes'] = ceil_to_minute(record['duration_minutes'])
            record['end_minutes'] = record['start_minutes'] + record['duration_minutes']
            record['end_time'] = record['start_time'] + timedelta(minutes=record['duration_minutes'])
        
        # Fix overlaps
        parsed_records = sorted(parsed_records, key=lambda x: x['start_minutes'])
        for i in range(1, len(parsed_records)):
            prev_record = parsed_records[i-1]
            current_record = parsed_records[i]
            
            if current_record['start_minutes'] < prev_record['end_minutes']:
                current_record['start_minutes'] = prev_record['end_minutes']
                current_record['start_time'] = prev_record['end_time']
                current_record['end_minutes'] = current_record['start_minutes'] + current_record['duration_minutes']
                current_record['end_time'] = current_record['start_time'] + timedelta(minutes=current_record['duration_minutes'])
    
    # apply custom processing
    parsed_records = custom_record_processing(parsed_records)
    
    # Get unique desktop names and assign colors
    unique_desktops = list(set(record['desktop'] for record in parsed_records))
    colors = generate_colors(len(unique_desktops))
    desktop_colors = dict(zip(unique_desktops, colors))
    
    # Calculate total time per desktop
    desktop_totals = defaultdict(float)
    for record in parsed_records:
        desktop_totals[record['desktop']] += record['duration_minutes']
    
    # Create the plot with better spacing
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=figsize, width_ratios=[2, 1], 
                                   gridspec_kw={'wspace': 0.5})
    
    # Timeline visualization (left plot) - vertical timeline
    x_pos = 0
    bar_width = 0.8
    
    for record in parsed_records:
        color = desktop_colors[record['desktop']]
        height = record['duration_minutes']
        
        # Draw the bar (vertical orientation)
        rect = Rectangle((x_pos, record['start_minutes']), bar_width, height, 
                        facecolor=color, edgecolor='white', linewidth=0.5, alpha=0.8)
        ax1.add_patch(rect)
    
    # Set up timeline axis (vertical) - reversed so earliest time is at top
    ax1.set_ylim(24 * 60, 0)  # Reversed: earliest time (0) at top, latest (24*60) at bottom
    ax1.set_xlim(-0.1, 1.1)
    
    # Create time labels every 15 minutes
    time_ticks = np.arange(0, 24 * 60 + 1, 15)  # Every 15 minutes
    time_labels = []
    for minutes in time_ticks:
        hours = int(minutes // 60)
        mins = int(minutes % 60)
        time_labels.append(f"{hours:02d}:{mins:02d}")
    
    ax1.set_yticks(time_ticks)
    ax1.set_yticklabels(time_labels, fontsize=8)
    ax1.set_xticks([])
    
    # Set title based on detail mode
    detail_suffix = " (Detailed)" if detail else " (Consolidated)"
    ax1.set_title(f'Virtual Desktop Usage Timeline - {target_date}{detail_suffix}', fontsize=14, fontweight='bold')
    ax1.set_ylabel('Time of Day', fontsize=12)
    ax1.grid(True, axis='y', alpha=0.3, linestyle='--')
    
    # Add horizontal lines for hour markers
    for hour in range(0, 25):
        ax1.axhline(y=hour * 60, color='gray', alpha=0.5, linestyle='-', linewidth=0.5)
    
    # Summary bar chart (right plot) - sorted by duration with better spacing
    sorted_items = sorted(desktop_totals.items(), key=lambda x: x[1], reverse=True)
    desktop_names = [item[0] for item in sorted_items]
    durations = [item[1] / 60 for item in sorted_items]  # Convert to hours
    bar_colors = [desktop_colors[name] for name in desktop_names]
    
    # Create bars with more spacing
    bar_positions = range(len(desktop_names))
    bars = ax2.bar(bar_positions, durations, color=bar_colors, alpha=0.8, width=0.6)
    
    # Set up the axis with better spacing for labels
    ax2.set_xticks(bar_positions)
    ax2.set_xticklabels(desktop_names, rotation=45, ha='right', fontsize=9)
    ax2.set_ylabel('Hours Spent', fontsize=12)
    ax2.set_title('Total Time per Desktop\n(Sorted by Duration)', fontsize=12, fontweight='bold')
    ax2.grid(True, axis='y', alpha=0.3)
    
    # Add some padding to the top for value labels
    max_duration = max(durations) if durations else 1
    ax2.set_ylim(0, max_duration * 1.15)
    
    # Add value labels on bars
    for i, (bar, duration) in enumerate(zip(bars, durations)):
        height = bar.get_height()
        ax2.text(bar.get_x() + bar.get_width()/2, height + max_duration * 0.02, 
                f'{duration:.1f}h', ha='center', va='bottom', fontsize=8)
    
    # Create legend positioned below the timeline
    legend_elements = [Rectangle((0, 0), 1, 1, facecolor=desktop_colors[desktop], 
                                label=f'{desktop} ({desktop_totals[desktop]/60:.1f}h)')
                      for desktop in unique_desktops]
    
    ax1.legend(handles=legend_elements, loc='upper center', bbox_to_anchor=(0.5, -0.05), 
              fontsize=9, title='Desktop Activities', ncol=2)
    
    # Adjust layout to prevent overlapping
    plt.subplots_adjust(bottom=0.2)
    
    # Print summary statistics
    print(f"\n=== Desktop Usage Summary for {target_date} ===")
    total_minutes = sum(desktop_totals.values())
    print(f"Total tracked time: {total_minutes/60:.1f} hours")
    
    for desktop, minutes in sorted(desktop_totals.items(), key=lambda x: x[1], reverse=True):
        percentage = (minutes / total_minutes) * 100 if total_minutes > 0 else 0
        print(f"{desktop}: {minutes/60:.1f}h ({percentage:.1f}%)")
    
    # Print the daily timeline
    print_daily_timeline(parsed_records)
    
    # Generate Timely JavaScript file
    generate_timely_javascript(parsed_records, target_date)
    
    plt.show()
    
    return fig, parsed_records

def main():
    """Main function with command-line argument parsing."""
    parser = argparse.ArgumentParser(description='Visualize Virtual Desktop Usage')
    parser.add_argument('--detail', action='store_true', 
                       help='Show detailed view with all activities (default: consolidate activities < 5 minutes)')
    parser.add_argument('--date', type=str, 
                       help='Date to visualize in YYYY-MM-DD format (default: today)')
    parser.add_argument('--figsize', nargs=2, type=int, default=[16, 10],
                       help='Figure size as width height (default: 16 10)')
    
    args = parser.parse_args()
    
    # Convert figsize to tuple
    figsize = tuple(args.figsize)
    
    print(f"VirtualDesktop Usage Visualization")
    print(f"Date: {args.date or 'today'}")
    print(f"Mode: {'Detailed (all activities)' if args.detail else 'Consolidated (smallest activities merged first)'}")
    print(f"Figure size: {figsize}")
    print("=" * 60)
    
    # Run visualization
    visualize_desktop_usage(target_date=args.date, figsize=figsize, detail=args.detail)

# Example usage:
if __name__ == "__main__":
    main()
