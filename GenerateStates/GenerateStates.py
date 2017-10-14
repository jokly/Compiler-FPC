import csv
import os

CSV_NAME = 'States.csv'
TXT_NAME = 'States.txt'

path = os.path.dirname(os.path.realpath(__file__))

with open(path + os.sep + CSV_NAME) as csvfile:
    reader = csv.reader(csvfile, delimiter = ';')
    next(reader)

    with open(path + os.sep +TXT_NAME, 'w') as txtfile:
        print('{', file = txtfile)

        for row in reader:
            print('{', file = txtfile)
            print(','.join(row[1:len(row)]), file = txtfile)
            print('},', file = txtfile)

        print('}', file = txtfile)