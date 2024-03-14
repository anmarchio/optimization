import matplotlib.pyplot as plt
import csv
import numpy as np
from matplotlib.backends.backend_pdf import PdfPages
import matplotlib.ticker as plticker

def read_frequency():
    with open("frequency.txt") as freq:
        reader = csv.reader(freq, delimiter=",")
        f = next(reader)
        return f 

#same as loglog, just different tick steps...
def plot_semilogY():
    f = read_frequency()
    l = []
    with open("pipelinePercentError.txt") as file:
        reader = csv.reader(file, delimiter=',')
        for row in reader:
            p = plt.figure()
            f = map(float, f)
            row = map(float, row)
            plt.ylabel("e'")
            plt.xlabel("f(Hz)")
            plt.title("% error")
            ax = plt.subplot()
            ax.set_yscale("symlog")
            ax.set_xscale("log")
            ax.set_xticks([0, 10 **2, 10 ** 4, 10 ** 6, 10 ** 8, 10 ** 10])
            #ax.xaxis.set_major_locator(plticker.LinearLocator(4, [0, 10 ** 2, 10 ** 4, 10**9]))
            plt.plot(f, row, "r+")
            l.append(p)
        return l

def plot_semilogX():
    f = read_frequency()
    l = []
    with open("pipelinePercentError.txt") as file:
        reader = csv.reader(file, delimiter=',')
        for row in reader:
            p = plt.figure()
            f = map(float, f)
            row = map(float, row)
            plt.ylabel("e'")
            plt.xlabel("f(Hz)")
            plt.title("% error")
            plt.semilogx(f, row, "r+")
            l.append(p)
        return l

def plot_loglog():
    f = read_frequency()
    l = []
    with open("pipelinePercentError.txt") as file:
        reader = csv.reader(file, delimiter=',')
        for row in reader:
            p = plt.figure()
            f = map(float, f)
            row = map(float, row)
            plt.title("% error")
            plt.ylabel("e'")
            plt.xlabel("f(Hz)")
            ax = plt.subplot()
            ax.set_yscale("symlog")
            ax.set_xscale("log")
            plt.plot(f, row, "r+")
            l.append(p)
        return l
		
def plot_all_and_theory():
    f = read_frequency()
    l = []
	# plot e1
    with open("pipelinePercentError.txt") as file:
        reader = csv.reader(file, delimiter=',')
        for row in reader:
            p = plt.figure()
            f = map(float, f)
            row = map(float, row)
            plt.title("% error")
            plt.ylabel("e'")
            plt.xlabel("f(Hz)")
            ax = plt.subplot()
            ax.set_yscale("symlog")
            ax.set_xscale("log")
            plt.plot(f, row, "r+")
            l.append(p)
	# plot e2
        return l
        
def plot_pdf(plot, filename):
    pp = PdfPages(filename)
    for p in plot:
        pp.savefig(p)
    pp.close()

#plot1 = plot_semilogY()
#plot_pdf(plot1, "semilogY.pdf")

#plot1 = plot_all_and_theory()
#plot_pdf(plot1, "all_and_theory.pdf")

plot2 = plot_semilogX()
plot_pdf(plot2, "semilogX.pdf")

plot3 = plot_loglog()
plot_pdf(plot3, "loglog.pdf")
