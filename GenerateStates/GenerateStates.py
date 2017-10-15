import csv
import os

CSV_NAME = 'States.csv'
TXT_NAME = 'States.txt'

path = os.path.dirname(os.path.realpath(__file__))
tabs = ' ' * 12

with open(path + os.sep + CSV_NAME) as csvfile:
    reader = csv.reader(csvfile, delimiter = ';')
    next(reader)

    with open(path + os.sep +TXT_NAME, 'w') as txtfile:
        for row in reader:
            print(tabs + '{', file = txtfile)
            print(tabs + ','.join(row[1:len(row)]), file = txtfile)
            print(tabs + '},', file = txtfile)
