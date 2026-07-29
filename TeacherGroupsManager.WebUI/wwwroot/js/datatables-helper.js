(function () {
    const lang = (document.documentElement.lang || 'ar').toLowerCase();
    const current = lang.startsWith('fr') ? 'fr' : lang.startsWith('en') ? 'en' : 'ar';
    const messages = {
        ar: {
            processing: 'جاري التحميل',
            search: 'بحث',
            lengthMenu: 'عرض _MENU_ سجل',
            info: 'عرض _START_ إلى _END_ من أصل _TOTAL_ سجل',
            infoEmpty: 'عرض 0 إلى 0 من أصل 0 سجل',
            infoFiltered: '(تمت التصفية من أصل _MAX_ سجل)',
            loadingRecords: 'جاري التحميل',
            zeroRecords: 'لا توجد نتائج مطابقة',
            emptyTable: 'لا توجد بيانات',
            first: 'الأول',
            previous: 'السابق',
            next: 'التالي',
            last: 'الأخير',
            active: 'نشط',
            inactive: 'غير نشط',
            privateLesson: 'درس خاص',
            publicGroup: 'مجموعة عامة',
            groupLesson: 'درس للمجموعة بالكامل',
            paid: 'مدفوع',
            partiallyPaid: 'مدفوع جزئي',
            unpaid: 'غير مدفوع',
            details: 'تفاصيل',
            edit: 'تعديل',
            permissions: 'الصلاحيات',
            delete: 'حذف',
            confirmDelete: 'هل تريد حذف هذا السجل؟',
            locale: 'ar-EG',
            days: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']
        },
        en: {
            processing: 'Loading',
            search: 'Search',
            lengthMenu: 'Show _MENU_ records',
            info: 'Showing _START_ to _END_ of _TOTAL_ records',
            infoEmpty: 'Showing 0 to 0 of 0 records',
            infoFiltered: '(filtered from _MAX_ records)',
            loadingRecords: 'Loading',
            zeroRecords: 'No matching records found',
            emptyTable: 'No data available',
            first: 'First',
            previous: 'Previous',
            next: 'Next',
            last: 'Last',
            active: 'Active',
            inactive: 'Inactive',
            privateLesson: 'Private lesson',
            publicGroup: 'Public group',
            groupLesson: 'Full group lesson',
            paid: 'Paid',
            partiallyPaid: 'Partially paid',
            unpaid: 'Unpaid',
            details: 'Details',
            edit: 'Edit',
            permissions: 'Permissions',
            delete: 'Delete',
            confirmDelete: 'Do you want to delete this record?',
            locale: 'en-US',
            days: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
        },
        fr: {
            processing: 'Chargement',
            search: 'Rechercher',
            lengthMenu: 'Afficher _MENU_ enregistrements',
            info: 'Affichage de _START_ à _END_ sur _TOTAL_ enregistrements',
            infoEmpty: 'Affichage de 0 à 0 sur 0 enregistrement',
            infoFiltered: '(filtré depuis _MAX_ enregistrements)',
            loadingRecords: 'Chargement',
            zeroRecords: 'Aucun résultat correspondant',
            emptyTable: 'Aucune donnée disponible',
            first: 'Premier',
            previous: 'Précédent',
            next: 'Suivant',
            last: 'Dernier',
            active: 'Actif',
            inactive: 'Inactif',
            privateLesson: 'Leçon privée',
            publicGroup: 'Groupe public',
            groupLesson: 'Leçon pour tout le groupe',
            paid: 'Payé',
            partiallyPaid: 'Payé partiellement',
            unpaid: 'Non payé',
            details: 'Détails',
            edit: 'Modifier',
            permissions: 'Autorisations',
            delete: 'Supprimer',
            confirmDelete: 'Voulez-vous supprimer cet enregistrement ?',
            locale: 'fr-FR',
            days: ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi']
        }
    };
    const t = messages[current];

    window.appTexts = t;
    window.arabicDataTablesLanguage = {
        processing: t.processing,
        search: t.search,
        lengthMenu: t.lengthMenu,
        info: t.info,
        infoEmpty: t.infoEmpty,
        infoFiltered: t.infoFiltered,
        loadingRecords: t.loadingRecords,
        zeroRecords: t.zeroRecords,
        emptyTable: t.emptyTable,
        paginate: {
            first: t.first,
            previous: t.previous,
            next: t.next,
            last: t.last
        }
    };

    function text(key, fallback) {
        return t[key] || fallback;
    }

    function enhanceTableFilters(options) {
        const $searchButton = $(options.searchButtonSelector);
        const $filterPanel = $searchButton.closest('.panel');

        if (!$filterPanel.length || $filterPanel.data('tableFiltersEnhanced')) {
            return;
        }

        const $row = $filterPanel.children('.row').first();
        if (!$row.length) {
            return;
        }

        $filterPanel.data('tableFiltersEnhanced', true);

        const tableId = String(options.tableSelector || 'table').replace(/^[#.]*/, '') || 'table';
        const drawerId = `${tableId}-filters-drawer`;
        const $columns = $row.children().detach();
        const $textColumns = $columns.filter(function () {
            return $(this).find('input:not([type="hidden"]):not([type="date"]):not([type="number"])').length > 0;
        });
        const $primaryColumn = $textColumns.first();
        const textFilterSelectors = $textColumns
            .map(function () {
                const input = $(this).find('input, select').first();
                return input.length ? `#${input.attr('id')}` : null;
            })
            .get()
            .filter(Boolean);
        const textFilterLabels = $textColumns
            .map(function () {
                return $.trim($(this).find('.form-label').first().text());
            })
            .get()
            .filter(Boolean);

        const $drawerColumns = $columns.filter(function () {
            return !textFilterSelectors.includes(`#${$(this).find('input, select').first().attr('id')}`);
        });
        const $buttonColumn = $drawerColumns.filter(function () {
            return $(this).find(`${options.searchButtonSelector}, ${options.resetButtonSelector}, ${options.reloadButtonSelector}`).length > 0;
        }).first();
        const $filterColumns = $drawerColumns.filter(function () {
            return this !== $buttonColumn.get(0);
        });

        const $toolbar = $('<div class="app-table-toolbar"></div>');
        const $toolbarLeft = $('<div class="app-table-toolbar-left"></div>');
        const $toolbarRight = $('<div class="app-table-toolbar-right"></div>');
        const $drawer = $(`
            <aside class="app-filter-drawer" id="${drawerId}" aria-hidden="true">
                <div class="app-filter-drawer-header">
                    <h2>${text('filters', 'Filters')}</h2>
                    <button type="button" class="app-filter-close-btn" aria-label="${text('close', 'Close')}">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
                <div class="app-filter-drawer-body"></div>
                <div class="app-filter-drawer-footer"></div>
            </aside>
        `);
        const $backdrop = $('<div class="app-filter-backdrop" aria-hidden="true"></div>');
        const $filterButton = $(`
            <button type="button" class="app-filter-open-btn" aria-controls="${drawerId}" aria-expanded="false">
                <i class="fa-solid fa-sliders"></i>
                <span>${text('filters', 'Filter')}</span>
            </button>
        `);

        if ($primaryColumn.length) {
            $primaryColumn.addClass('app-search-input-wrap');
            const $label = $primaryColumn.find('.form-label').first();
            const $input = $primaryColumn.find('input').first();
            const searchPrefix = current === 'en' ? 'Search by' : text('search', 'Search');
            const placeholder = textFilterLabels.length
                ? `${searchPrefix} ${textFilterLabels.join(', ')}`
                : text('search', 'Search');
            $label.addClass('visually-hidden');
            $input
                .addClass('app-search-input')
                .attr('placeholder', placeholder)
                .attr('title', placeholder)
                .val('');
            $toolbarLeft.append($primaryColumn);
            options.globalSearchInput = $input;
            options.textFilterSelectors = textFilterSelectors;
            options.filters = Object.fromEntries(Object.entries(options.filters || {}).filter(([, selector]) => !textFilterSelectors.includes(selector)));
        }

        $toolbarLeft.append($filterButton);
        $toolbar.append($toolbarLeft, $toolbarRight);
        $filterPanel.empty().append($toolbar);

        $drawer.find('.app-filter-drawer-body').append($filterColumns);

        if ($buttonColumn.length) {
            const $buttons = $buttonColumn.find('button').detach();
            const resetText = $.trim($buttons.filter(options.resetButtonSelector).text()) || 'Reset';
            const reloadText = $.trim($buttons.filter(options.reloadButtonSelector).text()) || 'Reload';
            $buttons.filter(options.searchButtonSelector).removeClass().addClass('app-filter-apply-btn').text(text('apply', 'Apply'));
            $buttons.filter(options.resetButtonSelector)
                .removeClass()
                .addClass('app-filter-secondary-btn')
                .attr({ title: resetText, 'aria-label': resetText })
                .html(`<i class="fa-solid fa-rotate-left" aria-hidden="true"></i><span class="visually-hidden">${resetText}</span>`);
            $buttons.filter(options.reloadButtonSelector)
                .removeClass()
                .addClass('app-filter-icon-btn')
                .attr({ title: reloadText, 'aria-label': reloadText })
                .html(`<i class="fa-solid fa-rotate" aria-hidden="true"></i><span class="visually-hidden">${reloadText}</span>`);
            $drawer.find('.app-filter-drawer-footer').append($buttons);
        }

        $filterPanel.after($backdrop, $drawer);

        function openDrawer() {
            $filterButton.attr('aria-expanded', 'true');
            $drawer.addClass('is-open').attr('aria-hidden', 'false');
            $backdrop.addClass('is-open');
            document.body.classList.add('app-filter-drawer-open');
        }

        function closeDrawer() {
            $filterButton.attr('aria-expanded', 'false');
            $drawer.removeClass('is-open').attr('aria-hidden', 'true');
            $backdrop.removeClass('is-open');
            document.body.classList.remove('app-filter-drawer-open');
        }

        $filterButton.on('click', openDrawer);
        $drawer.find('.app-filter-close-btn').on('click', closeDrawer);
        $backdrop.on('click', closeDrawer);
        $drawer.find(options.searchButtonSelector).on('click', closeDrawer);

        $primaryColumn.find('input').on('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                $searchButton.trigger('click');
            }
        });

        $(document).on(`keydown.${tableId}Filters`, function (event) {
            if (event.key === 'Escape' && $drawer.hasClass('is-open')) {
                closeDrawer();
            }
        });
    }

    window.initServerDataTable = function (options) {
        const $tableElement = $(options.tableSelector);
        $tableElement.addClass('app-table');
        $tableElement.closest('.table-responsive').addClass('app-table-shell');
        enhanceTableFilters(options);

        const table = $(options.tableSelector).DataTable({
            processing: true,
            serverSide: true,
            searching: true,
            ordering: true,
            pageLength: 10,
            lengthChange: false,
            lengthMenu: [10, 25, 50, 100],
            layout: {
                topStart: null,
                topEnd: null,
                bottomStart: 'info',
                bottomEnd: 'paging'
            },
            ajax: {
                url: options.url,
                type: 'POST',
                data: function (data) {
                    data.filters = collectDataTableFilters(options.filters || {});
                    const token = $('input[name="__RequestVerificationToken"]').first().val();
                    if (token) {
                        data.__RequestVerificationToken = token;
                    }
                }
            },
            columns: options.columns,
            language: window.arabicDataTablesLanguage,
            direction: document.documentElement.dir || 'rtl',
            autoWidth: false,
            createdRow: function (row) {
                $(row).addClass('app-table-row').find('td').addClass('app-table-cell');
            },
            drawCallback: function () {
                $tableElement.find('tbody tr').addClass('app-table-row');
                $tableElement.find('tbody td').addClass('app-table-cell');
            }
        });

        $(options.searchButtonSelector).on('click', function () {
            table.search(options.globalSearchInput?.val() || '').draw();
        });

        $(options.resetButtonSelector).on('click', function () {
            Object.values(options.filters || {}).forEach(function (selector) {
                $(selector).val('');
            });
            options.globalSearchInput?.val('');
            table.search('');
            table.ajax.reload();
        });

        $(options.reloadButtonSelector).on('click', function () {
            table.ajax.reload(null, false);
        });

        if (options.globalSearchInput?.length) {
            let searchTimer = null;
            options.globalSearchInput.on('input', function () {
                window.clearTimeout(searchTimer);
                searchTimer = window.setTimeout(function () {
                    table.search(options.globalSearchInput.val() || '').draw();
                }, 450);
            });
        }

        return table;
    };

    window.collectDataTableFilters = function (filters) {
        const values = {};
        Object.keys(filters).forEach(function (key) {
            values[key] = $(filters[key]).val();
        });
        return values;
    };

    window.renderBooleanStatus = function (value) {
        return value
            ? `<span class="status-chip status-chip-success">${t.active}</span>`
            : `<span class="status-chip status-chip-muted">${t.inactive}</span>`;
    };

    window.renderDate = function (value) {
        if (!value) return '';
        return String(value).substring(0, 10);
    };

    window.renderMoney = function (value) {
        return Number(value || 0).toLocaleString(t.locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    };

    window.renderGroupType = function (value) {
        return Number(value) === 1 ? t.privateLesson : t.publicGroup;
    };

    window.renderLessonType = function (value) {
        return Number(value) === 1 ? t.privateLesson : t.groupLesson;
    };

    window.renderPaymentStatus = function (value) {
        switch (Number(value)) {
            case 3: return `<span class="status-chip status-chip-success">${t.paid}</span>`;
            case 2: return `<span class="status-chip status-chip-warning">${t.partiallyPaid}</span>`;
            default: return `<span class="status-chip status-chip-danger">${t.unpaid}</span>`;
        }
    };

    window.renderDayOfWeek = function (value) {
        return t.days[Number(value)] || '';
    };

    window.renderGroupSchedules = function (value, type, row) {
        const schedules = Array.isArray(row.schedules) && row.schedules.length ? row.schedules : [{
            dayOfWeek: row.dayOfWeek,
            startTime: row.startTime,
            endTime: row.endTime
        }];

        return schedules.map(function (schedule) {
            return `${window.renderDayOfWeek(schedule.dayOfWeek)} ${schedule.startTime} - ${schedule.endTime}`;
        }).join('<br>');
    };

    window.initGroupSchedulesForm = function (formSelector, tableSelector, addButtonSelector) {
        const $form = $(formSelector);
        const $table = $(tableSelector);
        const dayOptions = t.days.map(function (day, index) {
            return `<option value="${index}">${day}</option>`;
        }).join('');

        function reindexSchedules() {
            $table.find('tbody tr').each(function (index) {
                $(this).find('.schedule-day').attr('name', `Schedules[${index}].DayOfWeek`);
                $(this).find('.schedule-start').attr('name', `Schedules[${index}].StartTime`);
                $(this).find('.schedule-end').attr('name', `Schedules[${index}].EndTime`);
            });
            syncPrimarySchedule();
        }

        function syncPrimarySchedule() {
            const $first = $table.find('tbody tr:first');
            $('#PrimaryDayOfWeek').val($first.find('.schedule-day').val() || '6');
            $('#PrimaryStartTime').val($first.find('.schedule-start').val() || '18:00');
            $('#PrimaryEndTime').val($first.find('.schedule-end').val() || '20:00');
        }

        $(addButtonSelector).on('click', function () {
            $table.find('tbody').append(`<tr>
                <td><select class="form-select schedule-day">${dayOptions}</select></td>
                <td><input class="form-control schedule-start" type="time" value="18:00" /></td>
                <td><input class="form-control schedule-end" type="time" value="20:00" /></td>
                <td><button class="btn btn-outline-danger btn-sm remove-schedule" type="button">${t.delete}</button></td>
            </tr>`);
            reindexSchedules();
        });

        $table.on('click', '.remove-schedule', function () {
            if ($table.find('tbody tr').length <= 1) return;
            $(this).closest('tr').remove();
            reindexSchedules();
        });

        $table.on('change', '.schedule-day, .schedule-start, .schedule-end', syncPrimarySchedule);
        $form.on('submit', function () {
            reindexSchedules();
            syncPrimarySchedule();
        });
    };

    window.renderActions = function (controller, id, includeDetails, permissionsUrl) {
        const details = includeDetails ? `<a class="app-table-action-btn" href="/${controller}/Details/${id}" title="${t.details}" aria-label="${t.details}"><i class="fa-solid fa-eye"></i></a> ` : '';
        const permissions = permissionsUrl ? `<a class="app-table-action-btn" href="${permissionsUrl}/${id}" title="${t.permissions}" aria-label="${t.permissions}"><i class="fa-solid fa-key"></i></a> ` : '';
        return `<div class="table-actions">
            ${details}<a class="app-table-action-btn" href="/${controller}/Edit/${id}" title="${t.edit}" aria-label="${t.edit}"><i class="fa-solid fa-pen"></i></a>
            ${permissions}<form action="/${controller}/Delete/${id}" method="post" class="d-inline">
                <input name="__RequestVerificationToken" type="hidden" value="${$('input[name="__RequestVerificationToken"]').first().val() || ''}" />
                <button class="app-table-action-btn app-table-action-danger" type="submit" title="${t.delete}" aria-label="${t.delete}" onclick="return confirm('${t.confirmDelete}')"><i class="fa-solid fa-trash-can"></i></button>
            </form>
        </div>`;
    };
})();
