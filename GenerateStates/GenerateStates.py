import csv

CSV_NAME = 'States.csv'
TXT_NAME = 'States.txt'

with open(CSV_NAME) as csvfile:
    reader = csv.reader(csvfile, delimiter = ';')
    next(reader)

    with open(TXT_NAME, 'w') as txtfile:
        print('{', file = txtfile)

        row_count = 0
        col_count = 0

        for row in reader:
            row_count += 1
            col_count = len(row)

            print('{', file = txtfile)
            print(','.join(row[1:len(row)]), file = txtfile)
            print('},', file = txtfile)

        print('}', file = txtfile)
        print('[{0}][{1}]'.format(row_count, col_count), file = txtfile)