import xlrd
import unicodecsv as csv
import os

def csv_from_excel():
       files = os.listdir(os.getcwd())
       csv_dir = os.getcwd() + "\\csv";
       if not os.path.exists(csv_dir):
              os.makedirs(csv_dir)
              
       for file in files:
              if not ".xls" in file:
                     continue
              wb = xlrd.open_workbook(file)
              sh = wb.sheet_by_index(0)
              your_csv_file = open(csv_dir + "\\" + file.replace('.xlsx', '.txt'), 'wb')
              wr = csv.writer(your_csv_file)

              for rownum in xrange(sh.nrows):
                     wr.writerow(sh.row_values(rownum))

              your_csv_file.close()


def exp_10(f):
       with open(f, 'rb') as csvfile:
              reader = csv.reader(csvfile, delimiter=',')
              with open("10.txt", 'wb') as writefile:
                     writer = csv.writer(writefile, delimiter=',')
                     for row in reader:
                            print(row[0].encode('ascii').strip(' \t\n\r'))
                            writer.writerow([10 ** float(row[0].encode('ascii').strip(' \t\n\r'))])
                     


csv_from_excel()
