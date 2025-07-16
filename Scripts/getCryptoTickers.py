import json
import requests
import time

def save_stocks_to_json(api_url):
    """
    Fetch stock tickers from Polygon API and save to JSON file
    
    Args:
        api_url (str): Complete API URL with parameters
    """
    all_stocks = []
    
    try:
        while api_url:
            # Make the API request
            response = requests.get(api_url)
            
            # Check for successful response
            response.raise_for_status()
            
            # Parse the JSON response
            data = response.json()
            
            # Extract ticker and name for each stock
            stocks = [
                {
                    "Ticker": stock['ticker'], 
                    "Name": stock['name']
                } 
                for stock in data['results']
            ]
            
            # Extend the all_stocks list
            all_stocks.extend(stocks)
            
            # Check if there's a next page
            api_url = data.get('next_url', '')

            if api_url:
                api_url += "&apiKey=tInlOScwTLtPvb9rzaQXVMaS_eWvtBb7"
            
            print(f"Fetched {len(all_stocks)} stocks so far...")
            print("Sleeping for 20 seconds")
            time.sleep(20)
    
    except requests.RequestException as e:
        print(f"Error fetching stocks: {e}")
        return
    
    # Save to a JSON file
    try:
        with open('crypto.json', 'w') as f:
            json.dump(all_stocks, f, indent=2)
        
        print(f"Successfully saved {len(all_stocks)} stocks to stocks.json")
    
    except IOError as e:
        print(f"Error saving to file: {e}")

# Example usage
if __name__ == "__main__":
    API_URL = "https://api.polygon.io/v3/reference/tickers?market=crypto&active=true&order=asc&limit=1000&sort=ticker&apiKey=tInlOScwTLtPvb9rzaQXVMaS_eWvtBb7"
    save_stocks_to_json(API_URL)