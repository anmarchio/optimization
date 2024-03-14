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

def plot_semilogX(num):
    f = read_frequency()
    l = []
    with open("pipelinePercentError" + str(num) + ".txt") as file:
        reader = csv.reader(file, delimiter=',')
        for row in reader:
            p = plt.figure()
            f = map(float, f)
            row = map(float, row)
            if num == 0:
                plt.ylabel("e'")
            else:
                plt.ylabel("e''")     
            plt.xlabel("f(Hz)")
            plt.title("% error")
            plt.semilogx(f, row, "r+")
            l.append(p)
        return l

def plot_loglog(num):
    f = read_frequency()
    l = []
    with open("pipelinePercentError" + str(num) + ".txt") as file:
        reader = csv.reader(file, delimiter=',')
        for row in reader:
            p = plt.figure()
            f = map(float, f)
            row = map(float, row)
            plt.title("% error")
            if num == 0:
                plt.ylabel("e'")
            else:
                plt.ylabel("e''")
            plt.xlabel("f(Hz)")
            ax = plt.subplot()
            ax.set_yscale("symlog")
            ax.set_xscale("log")
            plt.plot(f, row, "r+")
            l.append(p)
        return l
		
def plot_all_and_theory(num):
    f = read_frequency()
    l = []
	# plot e1
    with open("pipelinePercentError" + str(num) + ".txt") as file:
        if num == 0:
            theory = open("pipelineTheoryE1PercentError.txt")
        else:
            theory = open("pipelineTheoryE2PercentError.txt")

        theory_reader = csv.reader(theory, delimiter=',')
        reader = csv.reader(file, delimiter=',')
        for row in reader:
            p = plt.figure()
            f = map(float, f)
            row = map(float, row)
            t = next(theory_reader)
            t = map(float, t)
            plt.title("% error")
            if num == 0:
                plt.ylabel("e'")
            else:
                plt.ylabel("e''")
            plt.xlabel("f(Hz)")
            ax = plt.subplot()
            ax.set_yscale("symlog")
            ax.set_xscale("log")
            ax.plot(f, row, "r+", label="pipeline")
            ax.plot(f, t, "bs", label="model")
            legend = ax.legend()
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

plot = plot_all_and_theory(0)
plot_pdf(plot, "theory_e_dash.pdf")

plot = plot_all_and_theory(1)
plot_pdf(plot, "theory_e_double_dash.pdf")

plot = plot_semilogX(0)
plot_pdf(plot, "semilogX_e_dash.pdf")

plot = plot_semilogX(1)
plot_pdf(plot, "semilogX_e_double_dash.pdf")

plot = plot_loglog(0)
plot_pdf(plot, "loglog_e_dash.pdf")

plot = plot_loglog(1)
plot_pdf(plot, "loglog_e_double_dash.pdf")

