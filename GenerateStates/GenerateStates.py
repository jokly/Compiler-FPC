import csv

CSV_NAME = 'States.csv'
TXT_NAME = 'States.txt'

with open(CSV_NAME) as csvfile:
    reader = csv.reader(csvfile, delimiter = ';')
    next(reader)

    with open(TXT_NAME, 'w') as txtfile:
        print('{', file = txtfile)

        for row in reader:
            print('{', file = txtfile)
            print(','.join(row[1:len(row)]), file = txtfile)
            print('},', file = txtfile)

        print('}', file = txtfile)