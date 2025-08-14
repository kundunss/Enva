// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Password copy functionality
document.addEventListener('DOMContentLoaded', function() {
    // Password field click to copy
    document.querySelectorAll('.password-field').forEach(function(element) {
        element.addEventListener('click', function() {
            const password = this.getAttribute('data-password');
            navigator.clipboard.writeText(password).then(function() {
                // Show success message
                const originalText = element.innerHTML;
                element.innerHTML = 'Kopyalandı!';
                element.style.color = '#28a745';
                setTimeout(function() {
                    element.innerHTML = originalText;
                    element.style.color = '#007bff';
                }, 1000);
            }).catch(function(err) {
                console.error('Kopyalama hatası: ', err);
                alert('Şifre kopyalanamadı!');
            });
        });
    });

    // Search functionality
    const searchInput = document.getElementById('searchInput');
    const siteFilter = document.getElementById('siteFilter');
    
    if (searchInput) {
        searchInput.addEventListener('input', filterTable);
    }
    
    if (siteFilter) {
        siteFilter.addEventListener('change', filterTable);
    }
    
    function filterTable() {
        const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
        const selectedSite = siteFilter ? siteFilter.value.toLowerCase() : '';
        const table = document.querySelector('table tbody');
        
        if (table) {
            const rows = table.querySelectorAll('tr');
            
            rows.forEach(function(row) {
                const cells = row.querySelectorAll('td');
                let showRow = true;
                
                if (cells.length > 0) {
                    // Search in all text content
                    const rowText = row.textContent.toLowerCase();
                    if (searchTerm && !rowText.includes(searchTerm)) {
                        showRow = false;
                    }
                    
                    // Filter by site - find the site column dynamically
                    if (selectedSite && cells.length > 1) {
                        let siteColumnIndex = -1;
                        
                        // Check page type and set correct site column index
                        const currentPath = window.location.pathname.toLowerCase();
                        if (currentPath.includes('mailentries')) {
                            siteColumnIndex = 2; // Site is 3rd column (index 2)
                        } else if (currentPath.includes('inventoryitems')) {
                            siteColumnIndex = 4; // Site is 5th column (index 4)
                        } else if (currentPath.includes('systemhardware')) {
                            siteColumnIndex = cells.length - 2; // Site is second to last column
                        }
                        
                        if (siteColumnIndex >= 0 && siteColumnIndex < cells.length) {
                            const siteCell = cells[siteColumnIndex];
                            const siteText = siteCell.textContent.toLowerCase().trim();
                            if (!siteText.includes(selectedSite)) {
                                showRow = false;
                            }
                        }
                    }
                }
                
                row.style.display = showRow ? '' : 'none';
            });
        }
    }
});
